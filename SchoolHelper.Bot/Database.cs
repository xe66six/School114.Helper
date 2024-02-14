using System;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using SchoolHelper.Bot.Structs;

namespace SchoolHelper.Bot
{
    public class Database
    {
        private static Database _instance;
        public static Database Instance() => _instance ??= new Database();

        private readonly SQLiteConnection _connection;
        private Random _rnd = new Random();

        private Database()
        {
            var dbName = "database.sqlite";
            
            var builder = new SQLiteConnectionStringBuilder
            {
                DataSource = dbName
            };

            var dbExists = File.Exists(dbName);
            
            _connection = new SQLiteConnection(builder.ToString());
            _connection.Open();

            if (dbExists) return;

            var commands = new[]
            {
                "create table queue (userid bigint, key text, message_id int)",
                "create table users (userid bigint, name text default '', surname text default '', form int default 0, form_letter text default '', is_banned int default 0, UNIQUE(userid))",
                "create table posts (id int, owner bigint, content text, approved int default 0, creation_date datetime default CURRENT_TIMESTAMP)",
                "create table like_activity (userid bigint, postid int, post_action text)"
            };
            
            foreach (var command in commands)
            {
                using var cmd = new SQLiteCommand(command, _connection);
                cmd.ExecuteNonQuery();
            }
        }

        public void AddToQueue(long userId, string key, int messageId)
        {
            using var cmd = new SQLiteCommand("insert into queue(userid, key, message_id) values(@user, @key, @msg)", _connection);
            cmd.Parameters.AddWithValue("@user", userId);
            cmd.Parameters.AddWithValue("@key", key);
            cmd.Parameters.AddWithValue("@msg", messageId);
            cmd.ExecuteNonQuery();
        }

        public Query FindQuery(long userId)
        {
            using var cmd = new SQLiteCommand("select key, message_id from queue where userid = @user", _connection);
            cmd.Parameters.AddWithValue("@user", userId);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                return new Query
                {
                    Key = reader["key"] as string,
                    MessageId = (int)reader["message_id"]
                };
            }

            return default;
        }

        public void RemoveQueue(long userId)
        {
            using var cmd = new SQLiteCommand("delete from queue where userid = @user", _connection);
            cmd.Parameters.AddWithValue("@user", userId);
            cmd.ExecuteNonQuery();
        }

        public void AddUserIfNotExists(long userId)
        {
            using var cmd = new SQLiteCommand("insert or ignore into users(userid) values(@user)", _connection);
            cmd.Parameters.AddWithValue("@user", userId);
            cmd.ExecuteNonQuery();
        }

        public User GetUser(long userId)
        {
            User InlineReadUser(string command, long userid)
            {
                using var cmd = new SQLiteCommand(command, _connection);
                cmd.Parameters.AddWithValue("@user", userid);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return new User
                    {
                        UserId = userid,
                        Name = (string)reader["name"],
                        Surname = (string)reader["surname"],
                        Form = (int)reader["form"],
                        FormLetter = (string)reader["form_letter"],
                        IsBanned = (int)reader["is_banned"] == 1
                    };
                }

                return default;
            }
            
            var user = InlineReadUser("insert or ignore into users(userid) values(@user)", userId);
            return user.UserId > 0 ? user : InlineReadUser("select * from users where userid=@user", userId);
        }

        public void UpdateUserCredentials(long userId, string key, object value)
        {
            using var cmd = new SQLiteCommand($"update users set {key} = @val where userid = @user", _connection);
            cmd.Parameters.AddWithValue("@user", userId);
            cmd.Parameters.AddWithValue("@val", value);
            cmd.ExecuteNonQuery();
        }
        
        public void BanUser(long userid)
        {
            using var cmd = new SQLiteCommand("update users set is_banned=1 where userid=@user", _connection);
            cmd.Parameters.AddWithValue("@user", userid);
            cmd.ExecuteNonQuery();
        }

        public int CreatePost(long userId, string content)
        {
            using var cmd = new SQLiteCommand($"insert into posts(id, owner, content) values(@id, @user, @content)", _connection);
            
            var compressedContent = Post.CompressContent(content);
            var id = _rnd.Next(100000, 1000000);
            
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@user", userId);
            cmd.Parameters.AddWithValue("@content", compressedContent);
            cmd.ExecuteNonQuery();

            return id;
        }

        public Post FetchPost(int id)
        {
            using var cmd = new SQLiteCommand("select * from posts where id=@id", _connection);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return new Post
                {
                    Id = id,
                    OwnerId = (long)reader["owner"],
                    Content = Post.DecompressContent((string)reader["content"]),
                    Approved = (int)reader["approved"] == 1,
                };
            }

            return default;
        }
        
        public void ApprovePost(long id)
        {
            using var cmd = new SQLiteCommand("update posts set approved=1 where id=@id", _connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public long GetLikes(long id)
        {
            return GetPostAction(id, "like");
        }
        
        public long GetDislikes(long id)
        {
            return GetPostAction(id, "dislike");
        }
        
        public void AddLike(long id, long userid)
        {
            AddPostAction(id, userid, "like");
        }
        
        public void AddDislike(long id, long userid)
        {
            AddPostAction(id, userid, "dislike");
        }

        private long GetPostAction(long postId, string action)
        {
            using var cmd = new SQLiteCommand("select count(*) from like_activity where postid=@id and post_action=@act", _connection);
            cmd.Parameters.AddWithValue("@id", postId);
            cmd.Parameters.AddWithValue("@act", action);
            return (long)cmd.ExecuteScalar();
        }
        
        private void AddPostAction(long id, long userid, string action)
        {
            SQLiteCommand cmd;
            if (CheckUserAction(id, userid))
            {
                cmd = new SQLiteCommand("update like_activity set post_action = @act where postid = @id and userid = @userid",
                    _connection);
            }
            else
            {
                cmd = new SQLiteCommand("insert into like_activity(userid, postid, post_action) values(@userid, @id, @act)", _connection);
            }
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@userid", userid);
            cmd.Parameters.AddWithValue("@act", action);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        private bool CheckUserAction(long id, long userid)
        {
            using var cmd = new SQLiteCommand("select count(*) from like_activity where postid = @id and userid = @userid",
                _connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@userid", userid);
            return (long)cmd.ExecuteScalar() != 0;
        }

        public long CountPosts()
        {
            using var cmd = new SQLiteCommand("select count(*) from posts where approved = 1", _connection);
            return (long)cmd.ExecuteScalar();
        }
        
        public Post NextPost(long id)
        {
            return GetNearestPost("select * from posts where id > @id and approved = 1 order by id limit 1", id);
        }
        
        public Post PreviousPost(long id)
        {
            return GetNearestPost("select * from posts where id < @id and approved = 1 order by id desc limit 1", id);
        }

        public Post GetLatestPost()
        {
            return GetNearestPost("select * from posts where approved = 1 order by creation_date desc limit 1");
        }

        private Post GetNearestPost(string command, long? id = null)
        {
            using var cmd = new SQLiteCommand(command, _connection);
            if (id != null) cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return new Post
                {
                    Id = Convert.ToInt64(reader["id"]),
                    OwnerId = (long)reader["owner"],
                    Content = Post.DecompressContent((string)reader["content"]),
                    Approved = (int)reader["approved"] == 1,
                };
            }

            return default;
        }
    }
}