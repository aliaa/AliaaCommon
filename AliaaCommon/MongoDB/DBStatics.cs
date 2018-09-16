﻿using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AliaaCommon
{
    public static class DBStatics
    {
        public static bool WriteLogDefaultValue = true, UnifyCharsDefaultValue = true, UnifyNumsDefaultValue;

        internal static readonly string DEFAULT_CONN_STRING = ConfigurationManager.AppSettings["MongoConnString"];
        internal static readonly string DEFAULT_DB_NAME = ConfigurationManager.AppSettings["DBName"];
        internal static readonly bool DONT_SET_DICTIONARY_CONVENTION_TO_ARRAY_OF_DOCUMENTS = ConfigurationManager.AppSettings["setDictionaryConventionToArrayOfDocuments"] == "false";

        internal static IMongoDatabase DefaultDB = null;
        internal static Dictionary<string, object> Collections = new Dictionary<string, object>();
        
    }
}