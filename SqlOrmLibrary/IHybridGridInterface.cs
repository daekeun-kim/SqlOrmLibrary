using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlOrmLibrary
{
    public interface IHybridGridInterface
    {
        string SetDBconnectionInfo();//DB Privider 지정하기
        
        string SetSQL(string sWhere);       
 
        string[] SetFieldforDatetime(); // Date type column               

        bool Add();// DB insert 
        bool Edit(); // DB update
        bool Delete();// DB Delete

        bool BeforeLoad(string inMember);// DB에서 Load 전에 검색값 커스터마이징
        bool AfterLoad();// DB에서 Load 완료 후        

        //server side validation 
        bool ValidationAdd(out string sMsg);// Create validation 
        bool ValidationEdit(out string sMsg); // Edit validation 
        bool ValidationDelete(out string sMsg);// Delete validation 

        object Copy(); // 객체 copy 함수

    }

}
