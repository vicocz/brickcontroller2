using System.Threading.Tasks;
using BrickController2.CreationManagement;
using Moq;
using Newtonsoft.Json;
using SQLite;
using Xunit;

namespace BrickController2.Tests.CreationManagement;

public class ControllerAssignmentPersistenceTests
{
    [Fact]
    public void ExistingDatabaseMigratesAndAssignmentsSurviveReload()
    {
        using var db = new SQLiteConnection(":memory:");
        db.Execute("CREATE TABLE Creation (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name VARCHAR)");
        db.Execute("INSERT INTO Creation (Name) VALUES ('Existing')");
        db.CreateTable<Creation>();
        var creation = db.Table<Creation>().First();
        Assert.Null(creation.ControllerAssignmentId);
        creation.ControllerAssignmentId = "android:descriptor:stable";
        creation.ControllerAssignmentName = "Generic controller";
        db.Update(creation);
        var loaded = db.Find<Creation>(creation.Id);
        Assert.Equal(creation.ControllerAssignmentId, loaded.ControllerAssignmentId);
        Assert.Equal(creation.ControllerAssignmentName, loaded.ControllerAssignmentName);
        Assert.Equal("Existing", loaded.Name);
    }

    [Fact]
    public void AssignmentsAreLocalAndNotExportedToOtherTablets()
    {
        var creation = new Creation { Name = "Robot", ControllerAssignmentId = "local-id", ControllerAssignmentName = "Gamepad" };
        var imported = JsonConvert.DeserializeObject<Creation>(JsonConvert.SerializeObject(creation))!;
        Assert.Null(imported.ControllerAssignmentId);
        Assert.Equal("Robot", imported.Name);
    }

    [Fact]
    public async Task ManagerPersistsTheChoiceAndRollsBackOnFailure()
    {
        var repo = new Mock<ICreationRepository>();
        var manager = new CreationManager(repo.Object);
        var creation = new Creation();
        await manager.AssignControllerAsync(creation, "d1", "First");
        repo.Verify(r => r.UpdateCreationAsync(creation), Times.Once);
        repo.Setup(r => r.UpdateCreationAsync(creation)).ThrowsAsync(new System.InvalidOperationException());
        await Assert.ThrowsAsync<System.InvalidOperationException>(() => manager.AssignControllerAsync(creation, "d2", "Second"));
        Assert.Equal("d1", creation.ControllerAssignmentId);
        Assert.Equal("First", creation.ControllerAssignmentName);
    }
}
