﻿using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackService.IServices;
using TrackService.Models;

namespace TrackService.Services
{
    public class RethinkDbStore : IRethinkDbStore
    {
        private static IRethinkDbConnectionFactory _connectionFactory;
        private static RethinkDB R = RethinkDB.R;
        private string _dbName;

        public RethinkDbStore(IRethinkDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
            _dbName = connectionFactory.GetOptions().Database;
        }

        public void InitializeDatabase()
        {
            // database
            CreateDb(_dbName);

            // tables
            CreateTable(_dbName, nameof(vehicles));
            CreateTable(_dbName, nameof(coordinates));
            CreateTable(_dbName, nameof(archive_vehicles));
            CreateTable(_dbName, nameof(archive_coordinates));

            // indexes
            CreateIndex(_dbName, nameof(vehicles), nameof(vehicles.Vehicle_Num));
            CreateIndex(_dbName, nameof(coordinates), nameof(coordinates.Vehicle_Id));
            CreateIndex(_dbName, nameof(archive_vehicles), nameof(archive_vehicles.Vehicle_Num));
            CreateIndex(_dbName, nameof(archive_coordinates), nameof(archive_coordinates.Vehicle_Id));
        }

        protected void CreateDb(string dbName)
        {
            var conn = _connectionFactory.CreateConnection();
            var exists = R.DbList().Contains(db => db == dbName).Run(conn);

            if (!exists)
            {
                R.DbCreate(dbName).Run(conn);
                R.Db(dbName).Wait_().Run(conn);
            }
        }

        protected void DropDb(string dbName)
        {
            var conn = _connectionFactory.CreateConnection();
            var exists = R.DbList().Contains(db => db == dbName).Run(conn);

            if (exists)
            {
                R.DbDrop(dbName).Run(conn);
            }
        }

        protected void CreateTable(string dbName, string tableName)
        {
            var conn = _connectionFactory.CreateConnection();
            var exists = R.Db(dbName).TableList().Contains(t => t == tableName).Run(conn);
            if (!exists)
            {
                R.Db(dbName).TableCreate(tableName).Run(conn);
                R.Db(dbName).Table(tableName).Wait_().Run(conn);
            }
        }

        protected void CreateIndex(string dbName, string tableName, string indexName)
        {
            var conn = _connectionFactory.CreateConnection();
            var exists = R.Db(dbName).Table(tableName).IndexList().Contains(t => t == indexName).Run(conn);
            if (!exists)
            {
                R.Db(dbName).Table(tableName).IndexCreate(indexName).Run(conn);
                R.Db(dbName).Table(tableName).IndexWait(indexName).Run(conn);
            }
        }

        protected void DropTable(string dbName, string tableName)
        {
            var conn = _connectionFactory.CreateConnection();
            var exists = R.Db(dbName).TableList().Contains(t => t == tableName).Run(conn);
            if (exists)
            {
                R.Db(dbName).TableDrop(tableName).Run(conn);
            }
        }

        public void Reconfigure(int shards, int replicas)
        {
            var conn = _connectionFactory.CreateConnection();
            var tables = R.Db(_dbName).TableList().Run(conn);
            foreach (string table in tables)
            {
                R.Db(_dbName).Table(table).Reconfigure().OptArg("shards", shards).OptArg("replicas", replicas).Run(conn);
                R.Db(_dbName).Table(table).Wait_().Run(conn);
            }
        }

    }
}
