using FluentAssertions;
using BondValuator.Core.Services;

namespace BondValuator.UnitTests;

public class CsvParserTests
{
    [Fact]
    public void LoadBonds_ValidCsv_ReturnsCorrectRecords()
    {
        string csvContent =
@"BondID;Issuer;Rate;FaceValue;PaymentFrequency;Rating;Type;YearsToMaturity;DiscountFactor;DeskNotes
B001;Issuer1;5%;1000;Annual;AA;Bond;2;0.9;Note1
B002;Issuer2;3%;500;None;A;Zero-Coupon;1;0.95;Note2";

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        var bonds = CsvParser.LoadBonds(tempFile);

        bonds.Should().HaveCount(2);
        bonds[0].BondID.Should().Be("B001");
        bonds[1].Type.Should().Be("Zero-Coupon");

        File.Delete(tempFile);
    }

    [Fact]
    public void LoadBonds_EmptyLines_IgnoresThem()
    {
        string csvContent =
@"BondID;Issuer;Rate;FaceValue;PaymentFrequency;Rating;Type;YearsToMaturity;DiscountFactor;DeskNotes

B001;Issuer1;5%;1000;Annual;AA;Bond;2;0.9;Note1
";

        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, csvContent);

        var bonds = CsvParser.LoadBonds(tempFile);
        bonds.Should().HaveCount(1);
        bonds[0].BondID.Should().Be("B001");

        File.Delete(tempFile);
    }
}
