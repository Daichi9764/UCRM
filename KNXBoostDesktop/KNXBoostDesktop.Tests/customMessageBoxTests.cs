namespace KNXBoostDesktop.Tests;


[TestFixture]
[Apartment(ApartmentState.STA)] 
public class customMessageBoxTests
{
    [Test]
    public void Show_ReturnsTrueOnOkButtonClick()
    {
        bool? result = CustomMessageBox.Show("Test Message");
        Assert.IsTrue(result.HasValue && result.Value); // Vérifie que le résultat est vrai (true) pour le bouton Ok
    }

    [Test]
    public void Show_ReturnsFalseOnCancelButtonClick()
    {
        bool? result = CustomMessageBox.Show("Test Message");
        Assert.IsTrue(result.HasValue && !result.Value); // Vérifie que le résultat est faux (false) pour le bouton Cancel
    }
}