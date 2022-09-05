using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels
{
    public class Usage
    {
        [Column(IsPrimary = true,IsIdentity = true)]
        public int Id { get; set; }
        public FunctionType Function { get; set; }
        public DateTime CreatedTime { get; set; }
        public enum FunctionType
        {
            PhysicalExam,
            TeachingEvaluation,
            Report,
            Classrooms,
            Library,
            GymnasiumReg,
            PrivateRooms,
            Expenditures,
            Bank,
            Invoice,
            WasherInfo,
            QZYQ,
            DormScore,
            Electricity
        }
    }
}
