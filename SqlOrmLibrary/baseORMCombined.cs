using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace SqlOrmLibrary
{
    public abstract class baseORMCombined : IORMInterface
    {

        public baseORMCombined()
        {
        }

        public abstract string SetDBconnectionInfo();

        public abstract string SetTableName();

        public abstract string SetWhereQueryForCombinedPrimaryKey();

        public virtual string SetPrimaryField()
        {
            string sPrimariyField = "";
            return sPrimariyField;

        }
        public virtual string SetPrimaryMemberVariable()
        {
            string sPrimariyField = "";
            return sPrimariyField;

        }

        public virtual string[] SetFieldforNotEdit()
        {
            string[] arNotEditFd = { "" };
            return arNotEditFd;
        }

        public virtual string SetQueryOption()
        {
            return "";
        }

        public virtual string SetPrimaryValue()
        {
            return "";
        }


        public virtual string[] SetFieldforDatetime()
        {
            string[] arDatePicker = { "" };
            return arDatePicker;
        }


        public virtual bool BeforeDelete()
        {
            return true;
        }
        public virtual bool BeforeAdd()
        {

            return true;
        }

        public virtual bool BeforeEdit()
        {

            return true;

        }
        public virtual bool AfterAdd()
        {
            return true;
        }
        public virtual bool AfterEdit()
        {
            return true;
        }
        public virtual bool AfterLoad()
        {

            return true;
        }
        public virtual bool AfterDelete()
        {
            return true;
        }

        public virtual bool isCombinedPrimaryKey()
        {
            return true;

        }
        



        public virtual string[] SetVirtualField()
        {
            string[] sVirtual = { };
            return sVirtual;
        }


        public virtual string[] SetFieldforByteType()
        {
            string[] sVirtual = { };
            return sVirtual;
        }

        public virtual void OnError(Exception ex , string resultSQL)
        {
        }

        public virtual void OnComplete(string resultSQL)
        {

        }

        //mapper 사용하기 위해선 반드시 있어야 함
        public object Copy()
        {
            return base.MemberwiseClone();
        }

    }
}