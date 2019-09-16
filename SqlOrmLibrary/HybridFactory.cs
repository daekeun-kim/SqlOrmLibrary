using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Data;
using System.Text.RegularExpressions;

namespace SqlOrmLibrary
{
    public class HybridFactory<T> where T : new()
    {
        private T _target;
        private string _DBServerName;

        public HybridFactory()
        {
            _target = new T();
            IHybridGridInterface itTarget = _target as IHybridGridInterface;
            _DBServerName = itTarget.SetDBconnectionInfo();
        }

        public HybridFactory(string DBServerNum)
        {
            _target = new T();
            _DBServerName = DBServerNum;
        }

        public HybridFactory(T target, string DBServerNum)
        {
            _target = target;
            _DBServerName = DBServerNum;
        }

        public void Put(T target)
        {
            _target = target;
        }

        public List<T> Load()
        {
            List<T> resultList = new List<T>();

            IHybridGridInterface itTarget = _target as IHybridGridInterface;            
            string sTableName = "garbage";
            string sWhere = "";
            string sSql = itTarget.SetSQL(sWhere);

            sSql = Regex.Replace(sSql, "rowid", "ROWID as ROW_ID", RegexOptions.IgnoreCase);

            EntityMapper omapper = new EntityMapper();
            omapper.oDB = new clsDBControl_new(_DBServerName); //데이터 베이스 정보
            omapper.Table_entity.Add(sTableName, _target); // 테이블 정보            
            omapper.Load(sSql); //데이터 쿼리 실행

            // 데이터 타입 형 변환
            for (int i = 0; i < omapper.Result[0].Count; i++)
            {
                resultList.Add((T)omapper.Result[0][i]);
            }

            // load 완료후 
            for (int j = 0; j < resultList.Count; j++)
            {
                IORMInterface itResult = resultList[j] as IORMInterface;

                //date type 변환
                string[] arDateTimeFields = itResult.SetFieldforDatetime();
                Type Class = resultList[j].GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < Properties.Length; i++)
                {

                    string table_field = table_field = Properties[i].Name.ToString();

                    if (arDateTimeFields.Contains(table_field))
                    {
                        object sRowValue = Properties[i].GetValue(resultList[j], null);
                        string sInputDate = DateTime.ParseExact(sRowValue.ToString(), "yyyy-MM-dd tt h:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal).ToString("yyyy/MM/dd HH:mm:ss");
                        Properties[i].SetValue(resultList[j], sInputDate, null);
                    }

                }

                itResult.AfterLoad();
            }

            //리스트 형태로 반환
            return resultList;
        }

        public List<T> Load(int ipage, int numberOfData , string iWhere)
        {
            List<T> resultList = new List<T>();

            IHybridGridInterface itTarget = _target as IHybridGridInterface;            
            string sTableName = "garbage";
            string sSql = itTarget.SetSQL(iWhere);

            sSql = Regex.Replace(sSql, "rowid", "ROWID as ROW_ID", RegexOptions.IgnoreCase);

            EntityMapper omapper = new EntityMapper();

            omapper.oDB = new clsDBControl_new(_DBServerName); //데이터 베이스 정보
            omapper.Table_entity.Add(sTableName, _target); // 테이블 정보           
            omapper.Load(sSql ,ipage, numberOfData); //데이터 쿼리 실행

            // 데이터 타입 형 변환
            for (int i = 0; i < omapper.Result[0].Count; i++)
            {
                resultList.Add((T)omapper.Result[0][i]);
            }

            // load 완료후 
            for (int j = 0; j < resultList.Count; j++)
            {
                IHybridGridInterface itResult = resultList[j] as IHybridGridInterface;

                //date type 변환
                string[] arDateTimeFields = itResult.SetFieldforDatetime();
                Type Class = resultList[j].GetType(); //객체의 타입 가져오기
                PropertyInfo[] Properties = Class.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                for (int i = 0; i < Properties.Length; i++)
                {

                    string table_field = table_field = Properties[i].Name.ToString();

                    if (arDateTimeFields.Contains(table_field))
                    {
                        object sRowValue = Properties[i].GetValue(resultList[j], null);
                        string sInputDate = "";
                        if (sRowValue != null && sRowValue.ToString() != "")
                        {
                            sInputDate = DateTime.ParseExact(sRowValue.ToString(), "yyyy-MM-dd tt h:mm:ss", null, System.Globalization.DateTimeStyles.AssumeLocal).ToString("yyyy/MM/dd HH:mm:ss");                            
                        }

                        Properties[i].SetValue(resultList[j], sInputDate, null);
                    }

                }

                itResult.AfterLoad();
            }

            //리스트 형태로 반환
            return resultList;
        }



        public DataTable LoadDatatable()
        {
            List<T> resultList = new List<T>();

            IHybridGridInterface itTarget = _target as IHybridGridInterface;
            string sTableName = "garbage";
            string sWhere = "";
            string sSql = itTarget.SetSQL(sWhere);

            sSql = Regex.Replace(sSql, "rowid", "ROWID as ROW_ID", RegexOptions.IgnoreCase);

            EntityMapper omapper = new EntityMapper();
            omapper.oDB = new clsDBControl_new(_DBServerName); //데이터 베이스 정보
            omapper.Table_entity.Add(sTableName, _target); // 테이블 정보            
            DataTable dtResult = omapper.LoadDatable(sSql); //데이터 쿼리 실행

            return dtResult;
        }

        public DataTable LoadDataTable(int ipage, int numberOfData, string iWhere)
        {

            IHybridGridInterface itTarget = _target as IHybridGridInterface;
            string sTableName = "garbage";
            string sSql = itTarget.SetSQL(iWhere);

            sSql = Regex.Replace(sSql, "rowid", "ROWID as ROW_ID", RegexOptions.IgnoreCase);

            EntityMapper omapper = new EntityMapper();

            omapper.oDB = new clsDBControl_new(_DBServerName); //데이터 베이스 정보
            omapper.Table_entity.Add(sTableName, _target); // 테이블 정보           
            DataTable dtResult = omapper.LoadDatable(sSql, ipage, numberOfData); //데이터 쿼리 실행

            return dtResult;
        }

        public string GetTotalCount( string iWhere)
        {
            clsDBControl_new oDBCon = new clsDBControl_new(_DBServerName); 
            IHybridGridInterface itTarget = _target as IHybridGridInterface;

            string sSql = itTarget.SetSQL(iWhere);

            string sTotalSql = "select count(*) from (" + sSql + " )";
            string TotalCount = oDBCon.QuerySingleData(sTotalSql);
            return TotalCount;

        }



    }
}