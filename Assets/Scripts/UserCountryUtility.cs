using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteDB;
using System.Text;
using System.Security.Cryptography;
using System;

public class UserCountryUtility
{

    public class User
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        public string PasswordHash { get; set; }
        public string UUID { get; set; }
        public int Budget { get; set; }
        public string PrimaryCounttryUUID { get; set; }
        public List<Country> Countries { get; set; }

    }
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UUID { get; set; }
        public string OwnerUUID { get; set; }
    }

    static public void ConfigureRaration()
    {
        BsonMapper.Global.Entity<User>().DbRef(x => x.Countries, "products");
    }

    static private string FILENAME="info.db";
    static private int StartBudget = 2000;
    static private string USERINFO = "USERINFO";
    static private string COUNTRYINFO = "COUNTRYINFO";

    static public User createUser(string directory,string userid, string password)
    {
        User info = new User();
        info.UserID = userid;
        info.PasswordHash = GetHashString(userid + password);
        info.UUID= Guid.NewGuid().ToString().Replace("-", "");
        info.Budget = StartBudget;//ここは改修する。
        var filepath = DataUtility.createFilePath(directory, FILENAME);
        ConfigureRaration();
        using (var db = new LiteDatabase(filepath))
        {
            var users = db.GetCollection<User>(USERINFO);

            var query = users.Find(x => x.UserID == userid);

            foreach (var c in query)
            {
                throw new Exception("UserID is already exist.");
            }

            users.EnsureIndex(x => x.UserID, true);
            users.Insert(info); 
        }
        return info;
    }
    static public User deleteUser(string directory,User user)
    {
        var filepath = DataUtility.createFilePath(directory, FILENAME);
        ConfigureRaration();
        using (var db = new LiteDatabase(filepath))
        {
            var users = db.GetCollection<User>(USERINFO);

            var query = users.Include(x => x.Countries).Find(x => x.UserID == user.UserID);

            foreach (User eachUser in query)
            {
                foreach (Country country in eachUser.Countries)
                {
                    country.OwnerUUID = null;
                    modifyCountry(directory,country);
                }
            }
            users.Insert(user);
        }
        return user;
    }
    static public User modifyUser(string directory, User user)
    {
        var filepath = DataUtility.createFilePath(directory, FILENAME);
        ConfigureRaration();
        using (var db = new LiteDatabase(filepath))
        {
            var users = db.GetCollection<User>(USERINFO);
            users.Update(user);
        }
        return user;
    }
    static public User getUserFromUUID(string directory, string uuid)
    {
        var filepath = DataUtility.createFilePath(directory, FILENAME);
        ConfigureRaration();
        using (var db = new LiteDatabase(filepath))
        {
            var users = db.GetCollection<User>(USERINFO);

            var query = users.Include(x => x.Countries).Find(x => x.UUID == uuid);

            foreach (User eachUser in query)
            {
                return eachUser;
            }
        }
        return null;
    }
    static public User getUserFromUSERID(string directory, string userid)
    {
        var filepath = DataUtility.createFilePath(directory, FILENAME);
        ConfigureRaration();
        using (var db = new LiteDatabase(filepath))
        {
            var users = db.GetCollection<User>(USERINFO);

            var query = users.Include(x => x.Countries).Find(x => x.UserID == userid);

            foreach (User eachUser in query)
            {
                return eachUser;
            }
        }
        return null;
    }


    static public Country createCountry(string directory, string name, string owneruuid)
    {
        Country country = new Country();
        country.Name = name;
        country.UUID = Guid.NewGuid().ToString().Replace("-", "");
        country.OwnerUUID = owneruuid;

        var filepath = DataUtility.createFilePath(directory, FILENAME);
        ConfigureRaration();
        using (var db = new LiteDatabase(filepath))
        {
            var countries = db.GetCollection<Country>(COUNTRYINFO);
            var query = countries.Find(x => x.Name == name);

            foreach (var c in query)
            {
                throw new Exception("CountryName is already exist.");
            }
            countries.Insert(country);
        }
        return null;
    }
    static public Country modifyCountry(string directory, Country country)
    {
        var filepath = DataUtility.createFilePath(directory, FILENAME);
        ConfigureRaration();
        using (var db = new LiteDatabase(filepath))
        {
            var countries = db.GetCollection<Country>(COUNTRYINFO);
            countries.Update(country);
        }
        return country;
    }

    static public Country deleteCountry(string directory, Country country)
    {
        var filepath = DataUtility.createFilePath(directory, FILENAME);
        ConfigureRaration();
        using (var db = new LiteDatabase(filepath))
        {
            var countries = db.GetCollection<Country>(COUNTRYINFO);
            countries.Delete(country.Id);
        }
        return country;
    }
    static public User addCountry2User(string directory, User user,Country country)
    {
        user.Countries.Add(country);
        country.OwnerUUID = user.UUID;
        var filepath = DataUtility.createFilePath(directory, FILENAME);
        ConfigureRaration();
        using (var db = new LiteDatabase(filepath))
        {
            var users = db.GetCollection<User>(USERINFO);
            var countries = db.GetCollection<Country>(COUNTRYINFO);
            countries.Update(country);
            users.Update(user);
        }
        return user;
    }
    static public User deleteCountry2User(string directory, User user, Country country)
    {
        user.Countries.RemoveAll(x => x.UUID == country.UUID);
        country.OwnerUUID = null; 
        var filepath = DataUtility.createFilePath(directory, FILENAME);
        ConfigureRaration();
        using (var db = new LiteDatabase(filepath))
        {
            var users = db.GetCollection<User>(USERINFO);
            var countries = db.GetCollection<Country>(COUNTRYINFO);
            countries.Update(country);
            users.Update(user);
        }
        return user;
    }
    static public User autehnticateUser(string directory, string userid, string password)
    {
        User user= getUserFromUSERID(directory, userid);
        try
        {
            if(user.PasswordHash == GetHashString(userid + password))
            {
                return user;
            } 
        }
        catch (Exception ex)
        {
            throw new Exception("UserID or Password is not correct");
        }
        return user;
    }
    public static string GetHashString(string text)
    {
        byte[] data = Encoding.UTF8.GetBytes(text);
        var algorithm = new SHA512CryptoServiceProvider();
        byte[] bs = algorithm.ComputeHash(data);
        algorithm.Clear();
        var result = new StringBuilder();
        foreach (byte b in bs)
        {
            result.Append(b.ToString("X2"));
        }
        return result.ToString();
    }
}
