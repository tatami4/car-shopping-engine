﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using DataTypes;

namespace Backend
{
    public class UserList
    {
        private List<User> userList;
        private FileReader userDataReader;
        private FileWriter userDataWriter;
        private Logger logger;

        public UserList(Logger logger, string userDbPath = null)
        {
            this.logger = logger;
            userDataReader = new FileReader(logger, userDbPath);
            userDataWriter = new FileWriter(logger, userDbPath);
            userList = userDataReader.GetAllUserData();
        }

        // returns null if not found
        private User GetUser(string username)
        {
            return userList.Find(user => user.Username == username);
        }

        public byte[] JsonGetUser(string username)
        {
            return JsonSerializer.SerializeToUtf8Bytes<User>(GetUser(username));
        }

        private bool AddUser(User user)
        {
            if (!CheckIfExists(user.Username))
            {
                userList.Add(user);
                userDataWriter.WriteUserData(user);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool JsonAddUser(byte[] user)
        {
            try
            {
                return AddUser(JsonSerializer.Deserialize<User>(user));
            }
            catch (Exception e)
            {
                logger.LogException(new BackendException("Cannot add user due to bad serialization", e));
                return false;
            }
        }

        public void DeleteUser(string username)
        {
            foreach (User user in userList)
            {
                if (user.Username == username)
                {
                    userList.Remove(user);
                    userDataWriter.DeleteUser(username);
                }
            }
        }

        public bool CheckIfExists(string username)
        {
            return userList.Exists(user => user.Username.Equals(username));
        }
    }
}
