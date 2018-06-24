﻿using AliaaCommon;
using MongoDB.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace AliaaCommon
{
    public class DataTableFactory
    {
        private readonly Type thisType;

        private DataTableFactory()
        {
            thisType = typeof(DataTableFactory);
        }

        protected DataTableFactory(Type thisType)
        {
            this.thisType = thisType;
        }

        protected static DataTableFactory instance = null;

        public static DataTableFactory GetInstance()
        {
            if (instance == null)
                instance = new DataTableFactory();
            return instance;
        }

        private Dictionary<Type, MethodInfo> methods = new Dictionary<Type, MethodInfo>();

        public DataTable Create<T>(bool convertDateToPersian = true, bool includeTimeInDates = true, string[] excludeColumns = null) where T : MongoEntity
        {
            return Create(DB<T>.GetAllAsEnumerable(), convertDateToPersian, includeTimeInDates, excludeColumns);
        }

        public DataTable Create<T>(IEnumerable<T> data, bool convertDateToPersian = true, bool includeTimeInDates = true, string[] excludeColumns = null) where T : MongoEntity
        {
            Type type = typeof(T);
            MethodInfo method;
            if (methods.ContainsKey(type))
                method = methods[type];
            else
            {
                method = thisType.GetMethod("For" + type.Name);
                methods.Add(type, method);
            }
            if (method == null)
                return CreateDataTable(new DataTable(), data, convertDateToPersian, includeTimeInDates, excludeColumns);
            return (DataTable)method.Invoke(this, new object[] { data, convertDateToPersian, includeTimeInDates, excludeColumns });
        }

        private static Dictionary<PropertyInfo, string> CreateDataTableColumns<T>(DataTable table, bool convertDateToPersian = true, bool includeTimeInDates = true, string[] excludeColumns = null)
        {
            Type ttype = typeof(T);
            PropertyInfo[] props = ttype.GetProperties();
            Dictionary<PropertyInfo, string> displayNames = new Dictionary<PropertyInfo, string>();
            foreach (PropertyInfo p in props)
            {
                if (excludeColumns != null)
                {
                    bool exclude = false;
                    foreach (string exCol in excludeColumns)
                    {
                        if (p.Name == exCol)
                        {
                            exclude = true;
                            break;
                        }
                    }
                    if (exclude)
                        continue;
                }
                string dispName = Utils.GetDisplayNameOfMember(p);
                displayNames.Add(p, dispName);
                if (table.Columns.Contains(dispName))
                    continue;

                Type propType = p.PropertyType;
                if (propType.IsEquivalentTo(typeof(ObjectId)) || propType.IsEnum || p.PropertyType.GetInterfaces().Contains(typeof(IEnumerable)))
                    propType = typeof(string);
                else if (propType == typeof(DateTime) && (!includeTimeInDates || convertDateToPersian))
                    propType = typeof(string);
                else
                {
                    Type undelying = Nullable.GetUnderlyingType(propType);
                    if (undelying != null)
                        propType = undelying;
                }
                DataColumn col = new DataColumn(dispName, propType);
                table.Columns.Add(col);
            }
            return displayNames;
        }

        public DataTable CreateDataTable<T>(DataTable table, IEnumerable<T> list, bool convertDateToPersian = true, bool includeTimeInDates = true, string[] excludeColumns = null)
        {
            if (list == null)
                return null;
            Dictionary<PropertyInfo, string> displayNames = CreateDataTableColumns<T>(table, convertDateToPersian, includeTimeInDates, excludeColumns);

            foreach (T item in list)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo p in displayNames.Keys)
                {
                    object value = p.GetValue(item);
                    if (value is ObjectId)
                    {
                        value = value.ToString();
                    }
                    else if (p.PropertyType.IsEnum)
                        value = Utils.GetDisplayNameOfMember(p.PropertyType, value.ToString());
                    else if (value is DateTime && convertDateToPersian)
                        value = Utils.GetPersianDateString((DateTime)value, includeTimeInDates);
                    else if (value is IEnumerable && !(value is string))
                    {
                        StringBuilder sb = new StringBuilder();
                        Type itemsType = null;
                        foreach (var i in (IEnumerable)value)
                        {
                            if (itemsType == null)
                                itemsType = i.GetType();
                            sb.Append(Utils.GetDisplayNameOfMember(itemsType, i.ToString())).Append(" ; ");
                        }
                        if (sb.Length > 3)
                            sb.Remove(sb.Length - 3, 3);
                        value = sb.ToString();
                    }
                    row[displayNames[p]] = value;
                }
                table.Rows.Add(row);
            }
            return table;
        }

    }
}