﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;
using System.Collections;

namespace SqlOrmLibrary
{
    internal class EntityMapperSQL
    {
        public Dictionary<string, dynamic> Table_entity { get; set; }
        public List<List<dynamic>> Result { get; set; }
        public string WhereCondition { get; set; }
        public clsSQLControl oDB { get; set; }
        public List<string> except_members { get; set; }
        public List<string> byte_type_members { get; set; }
        public string sSql { get; set; }

        public EntityMapperSQL()
        {
            except_members = new List<string>();
            byte_type_members = new List<string>();
            Table_entity = new Dictionary<string, dynamic>();
            Result = new List<List<dynamic>>();
        }

        public string Create(dynamic obj, string table_name)
        {
            Type t = obj.GetType(); //객체의 타입 가져오기
            //sql 구문 만들기 시작

            PropertyInfo[] Properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            //필요 없는 칼럼 제거
            var removed_properties = from prt in Properties
                                     where !except_members.Contains(prt.Name)
                                     select prt;

            Properties = removed_properties.ToArray();

            //insert into table명 (컬럼1,컬럽2,...)
            string sSql_insert = "insert into ";
            sSql_insert += table_name + "(";
            string sSql_values = "values (";

            for (int i = 0; i < Properties.Length; i++)
            {


                string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                string object_member = string.Empty;
                string table_field = string.Empty;
                string sTempValue = "";

                if (firstofField == "_")
                {
                    table_field = Properties[i].Name.ToString().Substring(1);
                    sTempValue = Properties[i].GetValue(obj, null);
                    if (sTempValue != null)
                    {
                        sTempValue = sTempValue.Replace("'", "''");
                    }
                    object_member = "'" + sTempValue + "'";
                }
                else
                {
                    table_field = Properties[i].Name.ToString();
                    object_member = Properties[i].GetValue(obj, null);

                    if (object_member != null)
                    {
                        object_member = object_member.Trim();
                    }
                    if (object_member == "" || object_member == null)
                    {
                        object_member = "'" + object_member + "'";

                    }
                }


                sSql_insert += table_field;
                sSql_values += object_member;

                if (i < Properties.Length - 1)
                {
                    sSql_insert += ",";
                    sSql_values += ",";
                }
                else
                {
                    sSql_insert += ")";
                    sSql_values += ")";
                }

            }
            sSql = sSql_insert + sSql_values;

            //sql 구문 만들기 끝
            return sSql;

        }
        public string Save(dynamic obj, string tableName, string WhereCondition)
        {
            Type t = obj.GetType(); //객체의 타입 가져오기
            //sql 구문 만들기 시작

            PropertyInfo[] Properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            //조회시 필요 없는 칼럼 제거
            var removed_properties = from prt in Properties
                                     where !except_members.Contains(prt.Name)
                                     select prt;

            Properties = removed_properties.ToArray();


            sSql = "update ";
            sSql += tableName + " set ";
            for (int i = 0; i < Properties.Length; i++)
            {
                string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                string object_member = string.Empty;
                string table_field = string.Empty;
                string sTempValue = "";

                if (firstofField == "_")
                {
                    table_field = Properties[i].Name.ToString().Substring(1);
                    sTempValue = Properties[i].GetValue(obj, null);
                    if (sTempValue != null)
                    {
                        sTempValue = sTempValue.Replace("'", "''");
                    }
                    object_member = "'" + sTempValue + "'";
                }
                else
                {
                    table_field = Properties[i].Name.ToString();
                    object_member = Properties[i].GetValue(obj, null);

                    if (object_member != null)
                    {
                        object_member = object_member.Trim();
                    }
                    if (object_member == "" || object_member == null)
                    {
                        object_member = "'" + object_member + "'";

                    }
                }


                sSql += table_field + " = " + object_member;
                if (i < Properties.Length - 1)
                {
                    sSql += ",";
                }
            }

            sSql += " " + WhereCondition;
            //sql 구문 만들기 끝
            return sSql;
        }

        //binding(parameterized) query 이용한 DB insert
        //객체의 string 타입외에 다른 값이 있을 경우 안됨
        //string으로 이루어진 객체만 가능
        public bool Create_Oracle(dynamic obj, string table_name)
        {
            bool isSucess = true;

            Type t = obj.GetType(); //객체의 타입 가져오기
            //sql 구문 만들기 시작

            PropertyInfo[] Properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            //필요 없는 칼럼 제거
            var removed_properties = from prt in Properties
                                     where !except_members.Contains(prt.Name)
                                     select prt;

            Properties = removed_properties.ToArray();

            //insert into table명 (컬럼1,컬럽2,...)
            string sSql_insert = "insert into ";
            sSql_insert += table_name + "(";
            string sSql_values = "values (";

            for (int i = 0; i < Properties.Length; i++)
            {


                string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                string object_member = string.Empty;
                string table_field = string.Empty;

                if (firstofField == "_")
                {
                    table_field = Properties[i].Name.ToString().Substring(1);
                    object_member = "@" + table_field.Trim();
                }
                else
                {
                    table_field = Properties[i].Name.ToString();
                    object_member = "@" + table_field.Trim();
                }


                sSql_insert += table_field;
                sSql_values += object_member;

                if (i < Properties.Length - 1)
                {
                    sSql_insert += ",";
                    sSql_values += ",";
                }
                else
                {
                    sSql_insert += ")";
                    sSql_values += ")";
                }

            }
            sSql = sSql_insert + sSql_values;

            isSucess = oDB.ExcuteNonQuerybyMapper(sSql, obj, this);

            //sql 구문 만들기 끝
            return isSucess;

        }

        //parameterized query List type
        //DB insert parameterize query
        public bool Create_Oracle_List(dynamic obj, string table_name)
        {
            bool isSucess = true;

            Type t = obj[0].GetType(); //객체의 타입 가져오기
            //sql 구문 만들기 시작

            PropertyInfo[] Properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            //필요 없는 칼럼 제거
            var removed_properties = from prt in Properties
                                     where !except_members.Contains(prt.Name)
                                     select prt;

            Properties = removed_properties.ToArray();

            //insert into table명 (컬럼1,컬럽2,...)
            string sSql_insert = "insert into ";
            sSql_insert += table_name + "(";
            string sSql_values = "values (";

            for (int i = 0; i < Properties.Length; i++)
            {


                string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                string object_member = string.Empty;
                string table_field = string.Empty;

                if (firstofField == "_")
                {
                    table_field = Properties[i].Name.ToString().Substring(1);
                    object_member = "@" + table_field.Trim();
                }
                else
                {
                    table_field = Properties[i].Name.ToString();
                    object_member = "@" + table_field.Trim();
                }


                sSql_insert += table_field;
                sSql_values += object_member;

                if (i < Properties.Length - 1)
                {
                    sSql_insert += ",";
                    sSql_values += ",";
                }
                else
                {
                    sSql_insert += ")";
                    sSql_values += ")";
                }

            }
            sSql = sSql_insert + sSql_values;

            isSucess = oDB.ExcuteNonQuerybyMapperListWithSingleStatement(sSql, obj, this);

            //sql 구문 만들기 끝
            return isSucess;

        }

        //binding(parameterized) query 이용한 DB update
        //객체의 string 타입외에 다른 값이 있을 경우 안됨
        //string으로 이루어진 객체만 가능
        public bool Save_Oracle(dynamic obj, string tableName, string WhereCondition)
        {
            bool isSucess = true;

            Type t = obj.GetType(); //객체의 타입 가져오기
            //sql 구문 만들기 시작

            PropertyInfo[] Properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            //조회시 필요 없는 칼럼 제거
            var removed_properties = from prt in Properties
                                     where !except_members.Contains(prt.Name)
                                     select prt;

            Properties = removed_properties.ToArray();


            sSql = "update ";
            sSql += tableName + " set ";
            for (int i = 0; i < Properties.Length; i++)
            {
                string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                string object_member = string.Empty;
                string table_field = string.Empty;

                if (firstofField == "_")
                {
                    table_field = Properties[i].Name.ToString().Substring(1);
                    object_member = "@" + table_field.Trim();
                }
                else
                {
                    table_field = Properties[i].Name.ToString();
                    object_member = "@" + table_field.Trim();
                }


                sSql += table_field + " = " + object_member;
                if (i < Properties.Length - 1)
                {
                    sSql += ",";
                }
            }

            sSql += " " + WhereCondition;

            isSucess = oDB.ExcuteNonQuerybyMapper(sSql, obj, this);
            //sql 구문 만들기 끝
            return isSucess;
        }

        //parameterized query List type
        //DB update parameterize query
        public bool Save_Oracle_List(dynamic obj, List<string> tableName, List<string> WhereCondition)
        {
            bool isSucess = true;

            List<string> rSql = new List<string>();

            for (int k = 0; k < obj.Count; k++)
            {

                Type t = obj[k].GetType(); //객체의 타입 가져오기
                //sql 구문 만들기 시작

                PropertyInfo[] Properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                //조회시 필요 없는 칼럼 제거
                var removed_properties = from prt in Properties
                                         where !except_members.Contains(prt.Name)
                                         select prt;

                Properties = removed_properties.ToArray();


                sSql = "update ";
                sSql += tableName[k] + " set ";
                for (int i = 0; i < Properties.Length; i++)
                {
                    string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                    string object_member = string.Empty;
                    string table_field = string.Empty;

                    if (firstofField == "_")
                    {
                        table_field = Properties[i].Name.ToString().Substring(1);
                        object_member = "@" + table_field.Trim();
                    }
                    else
                    {
                        table_field = Properties[i].Name.ToString();
                        object_member = "@" + table_field.Trim();
                    }


                    sSql += table_field + " = " + object_member;
                    if (i < Properties.Length - 1)
                    {
                        sSql += ",";
                    }
                }

                sSql += " " + WhereCondition[k];

                rSql.Add(sSql);
            }

            isSucess = oDB.ExcuteNonQuerybyMapperList(rSql, obj, this);
            //sql 구문 만들기 끝
            return isSucess;
        }

        public void Load()
        {
            //1. object to return 

            //2. start to write query 
            sSql = string.Empty;
            string Select_syntax = "select ";
            string From_syntax = " from ";

            //3. select 컬럼1,컬럼2,컬럼3 .... 만들기 
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                string alliance = "";
                string tableName = tablename.Key.TrimEnd();
                if (tableName.Contains(" "))
                {
                    string[] name = tableName.Split(' ');
                    alliance = name[name.Length - 1];
                    alliance = alliance + ".";
                }


                //select 구문 만들기 - 객체의 멤버 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                //필요 없는 칼럼 제거
                var removed_properties = from prt in Properties
                                         where !except_members.Contains(prt.Name)
                                         select prt;

                Properties = removed_properties.ToArray();

                for (int i = 0; i < Properties.Length; i++)
                {

                    string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                    string table_field = string.Empty;

                    if (firstofField == "_")
                    {
                        table_field = Properties[i].Name.ToString().Substring(1);
                    }
                    else
                    {
                        table_field = Properties[i].Name.ToString();
                    }


                    Select_syntax += alliance + table_field + ",";

                }


                //from 구문 만들기 - 테이블 명 가져오기 
                From_syntax += tablename.Key + ",";
            }
            Select_syntax = Select_syntax.Remove(Select_syntax.LastIndexOf(","));
            From_syntax = From_syntax.Remove(From_syntax.LastIndexOf(",")); // 마지막 ,문자 없애기

            sSql = Select_syntax + From_syntax + " " + WhereCondition;
            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            int k3 = 0; // 초기 컬럼 스타트 지점
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                List<dynamic> result = new List<dynamic>();
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                //필요 없는 칼럼 제거
                var removed_properties = from prt in Properties
                                         where !except_members.Contains(prt.Name)
                                         select prt;

                Properties = removed_properties.ToArray();

                int k2 = k3; //한 객체 row 칼럼 완전 iteration 돌고 난 다음 컬럼 스타트 지점
                for (int i = 0; i < oDT.Rows.Count; i++)
                {
                    int k1 = k2; // 객체 row 한번 iteration 돌고 난 다음 컬럼 스타트
                    for (int j = 0; j < Properties.Length; j++)
                    {
                        //ByteType의 경우
                        if (byte_type_members.Contains(Properties[j].Name.ToString()))
                        {
                            if (oDT.Rows[i][k1] != System.DBNull.Value)
                            {
                                byte[] binDate = (byte[])oDT.Rows[i][k1];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }
                            else
                            {
                                byte[] binDate = new byte[0];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }

                            k1++;
                            continue;
                        }

                        //객체의 멤버에 db필드값 지정
                        string member = oDT.Rows[i][k1].ToString();
                        Properties[j].SetValue(tablename.Value, member, null);
                        k1++;
                    }
                    k3 = k1; // 진행된 칼럼 순서 저장 

                    dynamic copyObject = tablename.Value.Copy();

                    result.Add(copyObject);

                }
                Result.Add(result);

            }

            //todo
        }

        public DataTable LoadDatable()
        {
            //1. object to return 

            //2. start to write query 
            sSql = string.Empty;
            string Select_syntax = "select ";
            string From_syntax = " from ";

            //3. select 컬럼1,컬럼2,컬럼3 .... 만들기 
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                string alliance = "";
                string tableName = tablename.Key.TrimEnd();
                if (tableName.Contains(" "))
                {
                    string[] name = tableName.Split(' ');
                    alliance = name[name.Length - 1];
                    alliance = alliance + ".";
                }


                //select 구문 만들기 - 객체의 멤버 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                //필요 없는 칼럼 제거
                var removed_properties = from prt in Properties
                                         where !except_members.Contains(prt.Name)
                                         select prt;

                Properties = removed_properties.ToArray();

                for (int i = 0; i < Properties.Length; i++)
                {

                    string firstofField = Properties[i].Name.ToString().Substring(0, 1);
                    string table_field = string.Empty;

                    if (firstofField == "_")
                    {
                        table_field = Properties[i].Name.ToString().Substring(1);
                    }
                    else
                    {
                        table_field = Properties[i].Name.ToString();
                    }


                    Select_syntax += alliance + table_field + ",";

                }


                //from 구문 만들기 - 테이블 명 가져오기 
                From_syntax += tablename.Key + ",";
            }
            Select_syntax = Select_syntax.Remove(Select_syntax.LastIndexOf(","));
            From_syntax = From_syntax.Remove(From_syntax.LastIndexOf(",")); // 마지막 ,문자 없애기

            sSql = Select_syntax + From_syntax + " " + WhereCondition;
            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            oDB.Close();
            return oDT;

        }

        public DataTable LoadDatable(int NumofPage, int NumofDatas)
        {
            string Page = NumofPage.ToString();
            string NumofData = NumofDatas.ToString();

            //2. start to write query 
            sSql = string.Empty;
            string Select_syntax = "select ";
            string From_syntax = " from ";

            //3. select 컬럼1,컬럼2,컬럼3 .... 만들기 
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                string alliance1 = "";       //ex abc
                string alliance2 = "";      //ex abc. 점이 있는 경우

                string tableName = tablename.Key.TrimEnd();
                if (tableName.Contains(" "))
                {
                    string[] name = tableName.Split(' ');
                    alliance1 = name[name.Length - 1];
                    alliance2 = alliance1 + ".";
                }

                if (alliance1 != "") // 테이블이 2개 이상일 경우
                {
                    alliance1 = " as " + alliance1;

                    //select 구문 만들기 - 객체의 멤버 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    //필요 없는 칼럼 제거
                    var removed_properties = from prt in Properties
                                             where !except_members.Contains(prt.Name)
                                             select prt;

                    Properties = removed_properties.ToArray();

                    for (int i = 0; i < Properties.Length; i++)
                    {
                        string firstofField = Properties[i].Name.ToString().Substring(0, 1);

                        string table_field = string.Empty;

                        if (firstofField == "_")
                        {
                            table_field = Properties[i].Name.ToString().Substring(1);

                        }
                        else
                        {
                            table_field = Properties[i].Name.ToString();

                        }


                        Select_syntax += alliance2 + table_field + alliance1 + table_field + ",";

                    }
                }
                else  // 테이블이 1개 일 경우
                {
                    //select 구문 만들기 - 객체의 멤버 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    //필요 없는 칼럼 제거
                    var removed_properties = from prt in Properties
                                             where !except_members.Contains(prt.Name)
                                             select prt;

                    Properties = removed_properties.ToArray();

                    for (int i = 0; i < Properties.Length; i++)
                    {
                        string firstofField = Properties[i].Name.ToString().Substring(0, 1);

                        string table_field = string.Empty;

                        if (firstofField == "_")
                        {
                            table_field = Properties[i].Name.ToString().Substring(1);


                        }
                        else
                        {
                            table_field = Properties[i].Name.ToString();

                        }

                        Select_syntax += table_field + ",";
                    }


                }


                //from 구문 만들기 - 테이블 명 가져오기 
                From_syntax += tablename.Key + ",";
            }
            Select_syntax = Select_syntax.Remove(Select_syntax.LastIndexOf(","));
            From_syntax = From_syntax.Remove(From_syntax.LastIndexOf(",")); // 마지막 ,문자 없애기

            // paging query 만들기 시작
            sSql = Select_syntax + From_syntax + " " + WhereCondition;

            string rear_paging_query = "";
            rear_paging_query += " OFFSET ((" + Page + "-1)*" + NumofData + ") ROWS ";
            rear_paging_query += " FETCH NEXT (" + Page + "*" + NumofData + ") ROWS ONLY";

            //paging query 완성
            sSql = sSql + rear_paging_query;

            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            oDB.Close();

            return oDT;
        }

        public DataTable LoadDatableRange(int StartValue, int EndValue)
        {
            string sStartValue = StartValue.ToString();
            string sEndValue = EndValue.ToString();

            //2. start to write query 
            sSql = string.Empty;
            string Select_syntax = "select ";
            string From_syntax = " from ";

            //3. select 컬럼1,컬럼2,컬럼3 .... 만들기 
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                string alliance1 = "";       //ex abc
                string alliance2 = "";      //ex abc. 점이 있는 경우

                string tableName = tablename.Key.TrimEnd();
                if (tableName.Contains(" "))
                {
                    string[] name = tableName.Split(' ');
                    alliance1 = name[name.Length - 1];
                    alliance2 = alliance1 + ".";
                }

                if (alliance1 != "") // 테이블이 2개 이상일 경우
                {
                    alliance1 = " as " + alliance1;

                    //select 구문 만들기 - 객체의 멤버 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    //필요 없는 칼럼 제거
                    var removed_properties = from prt in Properties
                                             where !except_members.Contains(prt.Name)
                                             select prt;

                    Properties = removed_properties.ToArray();

                    for (int i = 0; i < Properties.Length; i++)
                    {
                        string firstofField = Properties[i].Name.ToString().Substring(0, 1);

                        string table_field = string.Empty;

                        if (firstofField == "_")
                        {
                            table_field = Properties[i].Name.ToString().Substring(1);

                        }
                        else
                        {
                            table_field = Properties[i].Name.ToString();

                        }


                        Select_syntax += alliance2 + table_field + alliance1 + table_field + ",";

                    }
                }
                else  // 테이블이 1개 일 경우
                {
                    //select 구문 만들기 - 객체의 멤버 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    //필요 없는 칼럼 제거
                    var removed_properties = from prt in Properties
                                             where !except_members.Contains(prt.Name)
                                             select prt;

                    Properties = removed_properties.ToArray();

                    for (int i = 0; i < Properties.Length; i++)
                    {
                        string firstofField = Properties[i].Name.ToString().Substring(0, 1);

                        string table_field = string.Empty;

                        if (firstofField == "_")
                        {
                            table_field = Properties[i].Name.ToString().Substring(1);

                            // RowID로 검색할 시에는 별칭을 만들어줌
                            if (table_field.ToUpper() == "ROWID")
                            {
                                table_field = "ROWID as ROW_ID";

                            }

                        }
                        else
                        {
                            table_field = Properties[i].Name.ToString();

                        }

                        Select_syntax += table_field + ",";
                    }


                }


                //from 구문 만들기 - 테이블 명 가져오기 
                From_syntax += tablename.Key + ",";
            }
            Select_syntax = Select_syntax.Remove(Select_syntax.LastIndexOf(","));
            From_syntax = From_syntax.Remove(From_syntax.LastIndexOf(",")); // 마지막 ,문자 없애기

            // paging query 만들기 시작

            sSql = Select_syntax + From_syntax + " " + WhereCondition;

            string rear_paging_query = "";
            rear_paging_query += " OFFSET (" + sStartValue + ") ROWS ";
            rear_paging_query += " FETCH NEXT (" + sEndValue + ") ROWS ONLY";

            //paging query 완성
            sSql =  sSql + rear_paging_query;

            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            oDB.Close();

            return oDT;
        }

        public DataTable LoadDatable(string input_sql)
        {
            sSql = input_sql;
            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];
            oDB.Close();

            return oDT;
            //todo
        }

        public DataTable LoadDatable(string input_sql, int NumofPage, int NumofDatas)
        {
            string Page = NumofPage.ToString();
            string NumofData = NumofDatas.ToString();

            // paging query 만들기 시작

            string rear_paging_query = "";
            rear_paging_query += " OFFSET ((" + Page + "-1)*" + NumofData + ") ROWS ";
            rear_paging_query += " FETCH NEXT (" + Page + "*" + NumofData + ") ROWS ONLY";

            //paging query 완성
            sSql =  input_sql + rear_paging_query;

            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            oDB.Close();

            return oDT;

        }

        public DataTable LoadDatableRange(string input_sql, int StartValue, int EndValue)
        {
            string sStartValue = StartValue.ToString();
            string sEndvalue = EndValue.ToString();

            // paging query 만들기 시작
            string rear_paging_query = "";
            rear_paging_query += " OFFSET (" + sStartValue + ") ROWS ";
            rear_paging_query += " FETCH NEXT (" + sEndvalue + ") ROWS ONLY";

            //paging query 완성
            sSql =  input_sql + rear_paging_query;

            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            oDB.Close();

            return oDT;

        }
        public void Load(string input_sql)
        {
            sSql = input_sql;
            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            int k3 = 0; // 초기 컬럼 스타트 지점
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                List<dynamic> result = new List<dynamic>();
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                //필요 없는 칼럼 제거
                var removed_properties = from prt in Properties
                                         where !except_members.Contains(prt.Name)
                                         select prt;

                Properties = removed_properties.ToArray();

                int k2 = k3; //한 객체 row 칼럼 완전 iteration 돌고 난 다음 컬럼 스타트 지점
                for (int i = 0; i < oDT.Rows.Count; i++)
                {
                    int k1 = k2; // 객체 row 한번 iteration 돌고 난 다음 컬럼 스타트
                    for (int j = 0; j < Properties.Length; j++)
                    {
                        //Byte 타입의 경우
                        if (byte_type_members.Contains(Properties[j].Name.ToString()))
                        {
                            if (oDT.Rows[i][k1] != System.DBNull.Value)
                            {
                                byte[] binDate = (byte[])oDT.Rows[i][k1];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }
                            else
                            {
                                byte[] binDate = new byte[0];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }

                            k1++;
                            continue;
                        }

                        //객체의 멤버에 db필드값 지정
                        string member = oDT.Rows[i][k1].ToString();
                        Properties[j].SetValue(tablename.Value, member, null);
                        k1++;
                    }
                    k3 = k1; // 진행된 칼럼 순서 저장 

                    dynamic copyObject = tablename.Value.Copy();

                    result.Add(copyObject);

                }
                Result.Add(result);

            }

            oDB.Close();

            //todo
        }
        public void Load(int NumofPage, int NumofDatas)
        {
            string Page = NumofPage.ToString();
            string NumofData = NumofDatas.ToString();

            //2. start to write query 
            sSql = string.Empty;
            string Select_syntax = "select ";
            string From_syntax = " from ";

            //3. select 컬럼1,컬럼2,컬럼3 .... 만들기 
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                string alliance1 = "";       //ex abc
                string alliance2 = "";      //ex abc. 점이 있는 경우

                string tableName = tablename.Key.TrimEnd();
                if (tableName.Contains(" "))
                {
                    string[] name = tableName.Split(' ');
                    alliance1 = name[name.Length - 1];
                    alliance2 = alliance1 + ".";
                }

                if (alliance1 != "") // 테이블이 2개 이상일 경우
                {
                    alliance1 = " as " + alliance1;

                    //select 구문 만들기 - 객체의 멤버 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    //필요 없는 칼럼 제거
                    var removed_properties = from prt in Properties
                                             where !except_members.Contains(prt.Name)
                                             select prt;

                    Properties = removed_properties.ToArray();

                    for (int i = 0; i < Properties.Length; i++)
                    {
                        string firstofField = Properties[i].Name.ToString().Substring(0, 1);

                        string table_field = string.Empty;

                        if (firstofField == "_")
                        {
                            table_field = Properties[i].Name.ToString().Substring(1);

                        }
                        else
                        {
                            table_field = Properties[i].Name.ToString();

                        }


                        Select_syntax += alliance2 + table_field + alliance1 + table_field + ",";

                    }
                }
                else  // 테이블이 1개 일 경우
                {
                    //select 구문 만들기 - 객체의 멤버 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    //필요 없는 칼럼 제거
                    var removed_properties = from prt in Properties
                                             where !except_members.Contains(prt.Name)
                                             select prt;

                    Properties = removed_properties.ToArray();

                    for (int i = 0; i < Properties.Length; i++)
                    {
                        string firstofField = Properties[i].Name.ToString().Substring(0, 1);

                        string table_field = string.Empty;

                        if (firstofField == "_")
                        {
                            table_field = Properties[i].Name.ToString().Substring(1);


                        }
                        else
                        {
                            table_field = Properties[i].Name.ToString();

                        }

                        Select_syntax += table_field + ",";
                    }


                }


                //from 구문 만들기 - 테이블 명 가져오기 
                From_syntax += tablename.Key + ",";
            }
            Select_syntax = Select_syntax.Remove(Select_syntax.LastIndexOf(","));
            From_syntax = From_syntax.Remove(From_syntax.LastIndexOf(",")); // 마지막 ,문자 없애기


            sSql = Select_syntax + From_syntax + " " + WhereCondition;
            string rear_paging_query = "";
            rear_paging_query += " OFFSET ((" + Page + "-1)*" + NumofData + ") ROWS ";            
            rear_paging_query += " FETCH NEXT (" + Page +"*" + NumofData + ") ROWS ONLY";

            //paging query 완성
            sSql = sSql + rear_paging_query;

            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            int k3 = 0; // 초기 컬럼 스타트 지점
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                List<dynamic> result = new List<dynamic>();
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                //필요 없는 칼럼 제거
                var removed_properties = from prt in Properties
                                         where !except_members.Contains(prt.Name)
                                         select prt;

                Properties = removed_properties.ToArray();

                int k2 = k3; //한 객체 row 칼럼 완전 iteration 돌고 난 다음 컬럼 스타트 지점
                for (int i = 0; i < oDT.Rows.Count; i++)
                {
                    int k1 = k2; // 객체 row 한번 iteration 돌고 난 다음 컬럼 스타트
                    for (int j = 0; j < Properties.Length; j++)
                    {
                        //Byte 타입의 경우
                        if (byte_type_members.Contains(Properties[j].Name.ToString()))
                        {
                            if (oDT.Rows[i][k1] != System.DBNull.Value)
                            {
                                byte[] binDate = (byte[])oDT.Rows[i][k1];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }
                            else
                            {
                                byte[] binDate = new byte[0];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }

                            k1++;
                            continue;
                        }

                        //객체의 멤버에 db필드값 지정
                        string member = oDT.Rows[i][k1].ToString();
                        Properties[j].SetValue(tablename.Value, member, null);
                        k1++;
                    }
                    k3 = k1; // 진행된 칼럼 순서 저장 

                    dynamic copyObject = tablename.Value.Copy();
                    result.Add(copyObject);

                }
                Result.Add(result);

            }

        }
        public void LoadRange(int StartValue, int EndValue)
        {
            string sStartValue = StartValue.ToString();
            string sEndValue = EndValue.ToString();

            //2. start to write query 
            sSql = string.Empty;
            string Select_syntax = "select ";
            string From_syntax = " from ";

            //3. select 컬럼1,컬럼2,컬럼3 .... 만들기 
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                string alliance1 = "";       //ex abc
                string alliance2 = "";      //ex abc. 점이 있는 경우

                string tableName = tablename.Key.TrimEnd();
                if (tableName.Contains(" "))
                {
                    string[] name = tableName.Split(' ');
                    alliance1 = name[name.Length - 1];
                    alliance2 = alliance1 + ".";
                }

                if (alliance1 != "") // 테이블이 2개 이상일 경우
                {
                    alliance1 = " as " + alliance1;

                    //select 구문 만들기 - 객체의 멤버 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    //필요 없는 칼럼 제거
                    var removed_properties = from prt in Properties
                                             where !except_members.Contains(prt.Name)
                                             select prt;

                    Properties = removed_properties.ToArray();

                    for (int i = 0; i < Properties.Length; i++)
                    {
                        string firstofField = Properties[i].Name.ToString().Substring(0, 1);

                        string table_field = string.Empty;

                        if (firstofField == "_")
                        {
                            table_field = Properties[i].Name.ToString().Substring(1);

                        }
                        else
                        {
                            table_field = Properties[i].Name.ToString();

                        }


                        Select_syntax += alliance2 + table_field + alliance1 + table_field + ",";

                    }
                }
                else  // 테이블이 1개 일 경우
                {
                    //select 구문 만들기 - 객체의 멤버 가져오기
                    PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    //필요 없는 칼럼 제거
                    var removed_properties = from prt in Properties
                                             where !except_members.Contains(prt.Name)
                                             select prt;

                    Properties = removed_properties.ToArray();

                    for (int i = 0; i < Properties.Length; i++)
                    {
                        string firstofField = Properties[i].Name.ToString().Substring(0, 1);

                        string table_field = string.Empty;

                        if (firstofField == "_")
                        {
                            table_field = Properties[i].Name.ToString().Substring(1);

                            // RowID로 검색할 시에는 별칭을 만들어줌
                            if (table_field.ToUpper() == "ROWID")
                            {
                                table_field = "ROWID as ROW_ID";

                            }

                        }
                        else
                        {
                            table_field = Properties[i].Name.ToString();

                        }

                        Select_syntax += table_field + ",";
                    }


                }


                //from 구문 만들기 - 테이블 명 가져오기 
                From_syntax += tablename.Key + ",";
            }
            Select_syntax = Select_syntax.Remove(Select_syntax.LastIndexOf(","));
            From_syntax = From_syntax.Remove(From_syntax.LastIndexOf(",")); // 마지막 ,문자 없애기

            // paging query 만들기 시작
            sSql = Select_syntax + From_syntax + " " + WhereCondition;
            
            string rear_paging_query = "";
            rear_paging_query += " OFFSET (" + sStartValue + ") ROWS ";
            rear_paging_query += " FETCH NEXT (" + sEndValue + ") ROWS ONLY";

            //paging query 완성
            sSql = sSql + rear_paging_query;

            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            int k3 = 0; // 초기 컬럼 스타트 지점
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                List<dynamic> result = new List<dynamic>();
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                //필요 없는 칼럼 제거
                var removed_properties = from prt in Properties
                                         where !except_members.Contains(prt.Name)
                                         select prt;

                Properties = removed_properties.ToArray();

                int k2 = k3; //한 객체 row 칼럼 완전 iteration 돌고 난 다음 컬럼 스타트 지점
                for (int i = 0; i < oDT.Rows.Count; i++)
                {
                    int k1 = k2; // 객체 row 한번 iteration 돌고 난 다음 컬럼 스타트
                    for (int j = 0; j < Properties.Length; j++)
                    {
                        //Byte 타입의 경우
                        if (byte_type_members.Contains(Properties[j].Name.ToString()))
                        {
                            if (oDT.Rows[i][k1] != System.DBNull.Value)
                            {
                                byte[] binDate = (byte[])oDT.Rows[i][k1];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }
                            else
                            {
                                byte[] binDate = new byte[0];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }

                            k1++;
                            continue;
                        }

                        //객체의 멤버에 db필드값 지정
                        string member = oDT.Rows[i][k1].ToString();
                        Properties[j].SetValue(tablename.Value, member, null);
                        k1++;
                    }
                    k3 = k1; // 진행된 칼럼 순서 저장 

                    dynamic copyObject = tablename.Value.Copy();
                    result.Add(copyObject);

                }
                Result.Add(result);

            }

        }
        public void Load(string input_sql, int NumofPage, int NumofDatas)
        {
            string Page = NumofPage.ToString();
            string NumofData = NumofDatas.ToString();


            string rear_paging_query = "";
            rear_paging_query += " OFFSET ((" + Page + "-1)*" + NumofData + ") ROWS ";
            rear_paging_query += " FETCH NEXT (" + Page + "*" + NumofData + ") ROWS ONLY";


            //paging query 완성
            sSql = input_sql + rear_paging_query;

            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            int k3 = 0; // 초기 컬럼 스타트 지점
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                List<dynamic> result = new List<dynamic>();
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                //필요 없는 칼럼 제거
                var removed_properties = from prt in Properties
                                         where !except_members.Contains(prt.Name)
                                         select prt;

                Properties = removed_properties.ToArray();

                int k2 = k3; //한 객체 row 칼럼 완전 iteration 돌고 난 다음 컬럼 스타트 지점
                for (int i = 0; i < oDT.Rows.Count; i++)
                {
                    int k1 = k2; // 객체 row 한번 iteration 돌고 난 다음 컬럼 스타트
                    for (int j = 0; j < Properties.Length; j++)
                    {
                        //ByteType의 경우
                        if (byte_type_members.Contains(Properties[j].Name.ToString()))
                        {
                            if (oDT.Rows[i][k1] != System.DBNull.Value)
                            {
                                byte[] binDate = (byte[])oDT.Rows[i][k1];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }
                            else
                            {
                                byte[] binDate = new byte[0];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }

                            k1++;
                            continue;
                        }

                        //객체의 멤버에 db필드값 지정
                        string member = oDT.Rows[i][k1].ToString();
                        Properties[j].SetValue(tablename.Value, member, null);
                        k1++;
                    }
                    k3 = k1; // 진행된 칼럼 순서 저장 

                    dynamic copyObject = tablename.Value.Copy();
                    result.Add(copyObject);

                }
                Result.Add(result);

            }

        }
        public void LoadRange(string input_sql, int StartValue, int EndValue)
        {
            string sStartValue = StartValue.ToString();
            string sEndValue = EndValue.ToString();

            // paging query 만들기 시작
            string rear_paging_query = "";
            rear_paging_query += " OFFSET (" + sStartValue + ") ROWS ";
            rear_paging_query += " FETCH NEXT (" + sEndValue + ") ROWS ONLY";

            //paging query 완성
            sSql =  input_sql + rear_paging_query;

            DataSet oDS = oDB.QueryDataSet(sSql);
            DataTable oDT = oDS.Tables[0];

            int k3 = 0; // 초기 컬럼 스타트 지점
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                List<dynamic> result = new List<dynamic>();
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                //필요 없는 칼럼 제거
                var removed_properties = from prt in Properties
                                         where !except_members.Contains(prt.Name)
                                         select prt;

                Properties = removed_properties.ToArray();

                int k2 = k3; //한 객체 row 칼럼 완전 iteration 돌고 난 다음 컬럼 스타트 지점
                for (int i = 0; i < oDT.Rows.Count; i++)
                {
                    int k1 = k2; // 객체 row 한번 iteration 돌고 난 다음 컬럼 스타트
                    for (int j = 0; j < Properties.Length; j++)
                    {
                        //ByteType의 경우
                        if (byte_type_members.Contains(Properties[j].Name.ToString()))
                        {
                            if (oDT.Rows[i][k1] != System.DBNull.Value)
                            {
                                byte[] binDate = (byte[])oDT.Rows[i][k1];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }
                            else
                            {
                                byte[] binDate = new byte[0];
                                Properties[j].SetValue(tablename.Value, binDate, null);
                            }

                            k1++;
                            continue;
                        }

                        //객체의 멤버에 db필드값 지정
                        string member = oDT.Rows[i][k1].ToString();
                        Properties[j].SetValue(tablename.Value, member, null);
                        k1++;
                    }
                    k3 = k1; // 진행된 칼럼 순서 저장 

                    dynamic copyObject = tablename.Value.Copy();
                    result.Add(copyObject);

                }
                Result.Add(result);

            }

        }
        public void LoadFromTable(DataTable in_DataTale)
        {
            DataTable oDT = in_DataTale;

            int k3 = 0; // 초기 컬럼 스타트 지점
            foreach (KeyValuePair<string, dynamic> tablename in Table_entity)
            {
                List<dynamic> result = new List<dynamic>();
                Type Class = tablename.Value.GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                //필요 없는 칼럼 제거
                var removed_properties = from prt in Properties
                                         where !except_members.Contains(prt.Name)
                                         select prt;

                Properties = removed_properties.ToArray();

                int k2 = k3; //한 객체 row 칼럼 완전 iteration 돌고 난 다음 컬럼 스타트 지점
                for (int i = 0; i < oDT.Rows.Count; i++)
                {
                    int k1 = k2; // 객체 row 한번 iteration 돌고 난 다음 컬럼 스타트
                    for (int j = 0; j < Properties.Length; j++)
                    {
                        //객체의 멤버에 db필드값 지정
                        string member = oDT.Rows[i][k1].ToString();
                        Properties[j].SetValue(tablename.Value, member, null);
                        k1++;
                    }
                    k3 = k1; // 진행된 칼럼 순서 저장 

                    dynamic copyObject = tablename.Value.Copy();

                    result.Add(copyObject);

                }
                Result.Add(result);

            }

            //todo
        }
        public string Delete(string table_name, string WhereCondition)
        {

            //insert into table명 (컬럼1,컬럽2,...)
            sSql = "Delete from ";
            sSql += table_name + " ";
            sSql += WhereCondition;

            //sql 구문 만들기 끝

            return sSql;

        }

        //sysdate로 입력된 값 수정할 때
        public string To_date(string date)
        {
            string todate = "to_date('" + date + "','YYYY-MM-DD AM HH:MI:SS')";

            return todate;
        }
        public void CopyObject(object source, object destination)
        {
            var props = source.GetType().GetProperties();

            foreach (var prop in props)
            {
                PropertyInfo info = destination.GetType().GetProperty(prop.Name);
                if (info != null)
                {
                    info.SetValue(destination, prop.GetValue(source, null), null);
                }
            }
        }
        public List<T> GetDistinctValues<T>(List<T> list)
        {
            List<T> tmp = new List<T>();
            for (int i = 0; i < list.Count; i++)
            {
                if (tmp.Contains(list[i]))

                    continue;
                tmp.Add(list[i]);
            }
            return tmp;
        }
        public List<TO_TYPE> AddRange<FROM_TYPE, TO_TYPE>(List<FROM_TYPE> listToCopyFrom, List<TO_TYPE> listToCopyTo) where FROM_TYPE : TO_TYPE
        {

            // loop through the list to copy, and  
            foreach (FROM_TYPE item in listToCopyFrom)
            {
                // add items to the copy tolist  
                listToCopyTo.Add(item);
            }

            // return the copy to list  
            return listToCopyTo;
        }

    }


}
