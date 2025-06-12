using FreeSql.DataAnnotations;

namespace ThuInfoWeb.DBModels;

public class Usage
{
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
        Electricity,
        NetworkDetail,
        OnlineDevices,
        SchoolCalendar,
        CampusCard,
        Income,
        DeepSeek,
    	Home,
    	News,
    	Schedule,
    	DeepSeekCreate,
    	DeepSeekSend,
    	DeepSeekSendRAG,
    	DeepSeekCopy,
    	DeepSeekRetry,
    	DeepSeekJump
    }

    [Column(IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }

    [Column(IsNullable = false)]
    public FunctionType Function { get; set; }

    [Column(IsNullable = false)]
    public DateTime CreatedTime { get; set; }

    [Column(IsNullable = true)]
    public Guid? Uuid { get; set; }
}
