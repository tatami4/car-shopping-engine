﻿using DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Backend
{
    public class Database : IDatabase
    {
        private List<Car> _carList;
        private List<User> _userList;
        private readonly List<MinimalUser> _minimalUserList = new List<MinimalUser>();
        private readonly FileReader _fileReader;
        private readonly FileWriter _fileWriter;
        private int _lastCarId;
        private readonly Logger _logger;

        public Database(Logger logger, string carDbPath = null, string userDbPath = null)
        {
            _logger = logger;
            _fileReader = new FileReader(logger, carDbPath, userDbPath);
            _fileWriter = new FileWriter(logger, carDbPath, userDbPath);
            LoadAllData();
        }

        private async void LoadAllData()
        {
            _carList = await _fileReader.GetAllCarData();
            _userList = await _fileReader.GetAllUserData();
            _lastCarId = _fileReader.LastCarId;
        }

        public int GetLastCarId()
        {
            return _lastCarId;
        }

        // params: Car serialized to JSON
        public bool AddCarJson(byte[] car)
        {
            try
            {
                Car c = JsonSerializer.Deserialize<Car>(car);
                c.Id = _lastCarId + 1;
                _lastCarId++;
                _carList.Add(c);
                _fileWriter.WriteCarData(c);
                _logger.Log("Added new car. ID = " + c.Id);
                return true;
            }
            catch (JsonException e)
            {
                _logger.LogException(new Exception("Failed to write car data due to bad serialization", e));
                return false;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return false;
            }
        }

        public bool AddUserJson(byte[] user)
        {
            try
            {
                User u = JsonSerializer.Deserialize<User>(user);
                if (!CheckIfExists(u.Username))
                {
                    _userList.Add(u);
                    // should return a status code, not bool
                    _logger.Log("Added new user. Username = " + u.Username);
                    return _fileWriter.WriteUserData(u);
                }
                else
                {
                    return false;
                }
            }
            catch (JsonException e)
            {
                _logger.LogException(new Exception("Cannot add user due to bad serialization", e));
                return false;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return false;
            }
        }

        // returns: MinimalUser serialized to UTF-8 JSON
        public byte[] Authenticate(string username, string hashedPassword)
        {
            try
            {
                User user = _userList.Find(user => user.Username == username && user.HashedPassword == hashedPassword);
                if (user == null)
                {
                    _logger.Log($"Failed login attempt. User [ {username} ] not found.");
                    return null;
                }
                else
                {
                    MinimalUser authenticatedUser = new MinimalUser
                    {
                        Username = user.Username,
                        Token = Guid.NewGuid().ToString(),
                        Phone = user.Phone,
                        Email = user.Email,
                        LikedAds = user.LikedAds
                    };
                    _minimalUserList.Add(authenticatedUser);
                    _logger.Log($"User [ {username} ] logged in.");
                    return JsonSerializer.SerializeToUtf8Bytes<MinimalUser>(authenticatedUser);
                }
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return null;
            }
        }

        public bool UpdateLikedAds(string token, List<int> newAds)
        {
            MinimalUser minimal = _minimalUserList.Find((minimalUser) => minimalUser.Token == token);
            if (minimal == null)
                return false;
            minimal.LikedAds = newAds;
            User original = _userList.Find((User user) => minimal.Username == user.Username);
            original.LikedAds = newAds;
            return true;
        }

        private bool CheckIfExists(string username)
        {
            return _userList.Exists(user => user.Username == username);
        }

        public bool DeleteCar(int id)
        {
            // remove this car from all user liked cars
            foreach (User user in _userList)
            {
                user.LikedAds.Remove(id);
            }
            return _carList.RemoveAll(car => car.Id == id) == 1 && _fileWriter.DeleteCar(id);
        }

        public bool DeleteUser(string username)
        {
            // remove ads posted by this user
            _carList.RemoveAll(car => car.UploaderUsername == username);
            return _userList.RemoveAll(user => user.Username == username) == 1 && _fileWriter.DeleteUser(username);
        }

        public byte[] GetFilteredCarsJson(CarFilters filters, SortingCriteria sortBy, bool sortAscending, int startIndex, int amount)
        {
            List<Car> filteredCarList = _carList;

            if (!string.IsNullOrEmpty(filters.Brand))
                filteredCarList = (from car in filteredCarList where car.Brand.ToLower().Contains(filters.Brand) select car).ToList();
            if (!string.IsNullOrEmpty(filters.Model))
                filteredCarList = (from car in filteredCarList where car.Model.ToLower().Contains(filters.Model) select car).ToList();
            if (filters.Used.HasValue)
                filteredCarList = (from car in filteredCarList where car.Used == filters.Used select car).ToList();
            if (filters.PriceFrom.HasValue)
                filteredCarList = (from car in filteredCarList where car.Price >= filters.PriceFrom select car).ToList();
            if (filters.PriceTo.HasValue)
                filteredCarList = (from car in filteredCarList where car.Price <= filters.PriceTo select car).ToList();
            if (filters.YearFrom.HasValue)
                filteredCarList = (from car in filteredCarList where car.DateOfPurchase.Year >= filters.YearFrom select car).ToList();
            if (filters.YearTo.HasValue)
                filteredCarList = (from car in filteredCarList where car.DateOfPurchase.Year <= filters.YearTo select car).ToList();
            if (filters.FuelType.HasValue)
                filteredCarList = (from car in filteredCarList where car.FuelType == filters.FuelType select car).ToList();

            // this is currently not used
            //if (!string.IsNullOrEmpty(filters.Username))
            //    filteredCarList = (from car in filteredCarList where car.UploaderUsername.ToLower() == filters.Username select car).ToList();


            return GetSortedCarsJson(sortBy, sortAscending, startIndex, amount, filteredCarList);
        }

        public byte[] GetSortedCarsJson(SortingCriteria sortBy, bool sortAscending, int startIndex, int amount)
        {
            return GetSortedCarsJson(sortBy, sortAscending, startIndex, amount, null);
        }

        // returns null if not found
        public byte[] GetUserInfoJson(string username)
        {
            User user = _userList.Find(user => user.Username == username);
            return user != null ? JsonSerializer.SerializeToUtf8Bytes<User>(user) : null;
        }

        public byte[] GetUserLikedAds(string username, int startIndex, int amount)
        {
            List<Car> likedCars = new List<Car>();
            User user = _userList.Find(user => user.Username == username);
            foreach (int id in user.LikedAds)
            {
                likedCars.Add(_carList.Find(car => car.Id == id));
            }

            List<Car> carsToReturn = likedCars.Skip(startIndex).Take(amount).ToList();
            return JsonSerializer.SerializeToUtf8Bytes(carsToReturn);
        }

        public byte[] GetUserUploadedAds(string username, int startIndex, int amount)
        {
            List<Car> uploadedCars = new List<Car>();
            User user = _userList.Find(user => user.Username == username);
            foreach (Car car in _carList)
            {
                if (car.UploaderUsername == username)
                {
                    uploadedCars.Add(car);
                }
            }

            List<Car> carsToReturn = uploadedCars.Skip(startIndex).Take(amount).ToList();
            return JsonSerializer.SerializeToUtf8Bytes(carsToReturn);
        }


        private byte[] GetSortedCarsJson(SortingCriteria sortBy, bool sortAscending, int startIndex, int amount, List<Car> carListToSort = null)
        {
            if (carListToSort == null)
            {
                carListToSort = _carList;
            }
            carListToSort.SortBy(sortBy, sortAscending);
            return JsonSerializer.SerializeToUtf8Bytes<List<Car>>(carListToSort.Skip(startIndex).Take(amount).ToList<Car>());
        }
    }
}
