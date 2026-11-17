using FreeSql;
using FreeSql.DataAnnotations;
using FreeSql.Internal;
using ThuInfoWeb.DBModels;
using Xunit;

namespace ThuInfoWeb.Tests;

public class DeepSeekModelNamesTests
{
    [Fact]
    public void NamesKeepTheirOrderAndCaseWhileRemovingWhitespaceEmptyItemsAndDuplicates()
    {
        Assert.Equal(["Model-B", "Model-A", "model-a"],
            DeepSeekModelNames.Parse(" Model-B, ,Model-A,Model-B,model-a,"));
        Assert.Equal("Model-B,Model-A", DeepSeekModelNames.Normalize(" Model-B,Model-A,Model-B, "));
        Assert.Empty(DeepSeekModelNames.Parse(null));
        Assert.Empty(DeepSeekModelNames.Parse(" , "));
    }

    [Fact]
    public void ExistingRowsReceiveTheDefaultOnceAndExplicitEmptyListsStayEmpty()
    {
        using var database = new FreeSqlBuilder()
            .UseConnectionString(DataType.Sqlite, "Data Source=:memory:")
            .UseAutoSyncStructure(true)
            .UseNameConvert(NameConvertType.ToLower)
            .Build();
        database.Insert(new LegacyMisc { Id = 1 }).ExecuteAffrows();
        database.CodeFirst.SyncStructure<Misc>();
        DeepSeekModelNames.Initialize(database);
        Assert.Equal(DeepSeekModelNames.DefaultModel, database.Select<Misc>().First().DeepSeekModels);

        database.Update<Misc>().Set(misc => misc.DeepSeekModels, "").Where(misc => misc.Id == 1).ExecuteAffrows();
        DeepSeekModelNames.Initialize(database);
        Assert.Empty(database.Select<Misc>().First().DeepSeekModels!);

        database.Update<Misc>().Set(misc => misc.DeepSeekModels, "Model-B,Model-A").Where(misc => misc.Id == 1).ExecuteAffrows();
        DeepSeekModelNames.Initialize(database);
        Assert.Equal(["Model-B", "Model-A"], DeepSeekModelNames.Parse(database.Select<Misc>().First().DeepSeekModels));
    }

    [Table(Name = "Misc")]
    private sealed class LegacyMisc
    {
        [Column(IsPrimary = true)]
        public int Id { get; init; }
    }
}
