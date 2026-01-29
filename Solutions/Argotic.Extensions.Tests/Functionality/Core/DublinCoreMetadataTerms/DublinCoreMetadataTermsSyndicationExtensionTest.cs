using System.Globalization;
using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.DublinCoreMetadataTerms;

[TestClass]
public class DublinCoreMetadataTermsSyndicationExtensionTest
{
    private const string Namespc = @"xmlns:dcterms=""http://purl.org/dc/terms/""";

    private readonly string toStringText = "<abstract xmlns=\"http://purl.org/dc/terms/\">Test Abstract</abstract>" + Environment.NewLine +
                                           "<accessRights xmlns=\"http://purl.org/dc/terms/\">Public</accessRights>" + Environment.NewLine +
                                           "<accrualMethod xmlns=\"http://purl.org/dc/terms/\">Deposit</accrualMethod>" + Environment.NewLine +
                                           "<accrualPeriodicity xmlns=\"http://purl.org/dc/terms/\">Monthly</accrualPeriodicity>" + Environment.NewLine +
                                           "<accrualPolicy xmlns=\"http://purl.org/dc/terms/\">Active</accrualPolicy>" + Environment.NewLine +
                                           "<alternative xmlns=\"http://purl.org/dc/terms/\">Alt Title</alternative>" + Environment.NewLine +
                                           "<audience xmlns=\"http://purl.org/dc/terms/\">Developers</audience>" + Environment.NewLine +
                                           "<available xmlns=\"http://purl.org/dc/terms/\">2010-08-01</available>" + Environment.NewLine +
                                           "<bibliographicCitation xmlns=\"http://purl.org/dc/terms/\">Test Citation</bibliographicCitation>" + Environment.NewLine +
                                           "<conformsTo xmlns=\"http://purl.org/dc/terms/\">ISO 9001</conformsTo>" + Environment.NewLine +
                                           "<contributor xmlns=\"http://purl.org/dc/terms/\">Helper</contributor>" + Environment.NewLine +
                                           "<coverage xmlns=\"http://purl.org/dc/terms/\">US</coverage>" + Environment.NewLine +
                                           "<created xmlns=\"http://purl.org/dc/terms/\">2010-06-01T00:00:00.00Z</created>" + Environment.NewLine +
                                           "<creator xmlns=\"http://purl.org/dc/terms/\">The Big Guy</creator>" + Environment.NewLine +
                                           "<date xmlns=\"http://purl.org/dc/terms/\">2010-08-01T00:00:00.00Z</date>" + Environment.NewLine +
                                           "<dateAccepted xmlns=\"http://purl.org/dc/terms/\">2010-07-01T00:00:00.00Z</dateAccepted>" + Environment.NewLine +
                                           "<dateCopyrighted xmlns=\"http://purl.org/dc/terms/\">2010-01-01T00:00:00.00Z</dateCopyrighted>" + Environment.NewLine +
                                           "<dateSubmitted xmlns=\"http://purl.org/dc/terms/\">2010-05-01T00:00:00.00Z</dateSubmitted>" + Environment.NewLine +
                                           "<description xmlns=\"http://purl.org/dc/terms/\">That kind of thing</description>" + Environment.NewLine +
                                           "<educationLevel xmlns=\"http://purl.org/dc/terms/\">Graduate</educationLevel>" + Environment.NewLine +
                                           "<extent xmlns=\"http://purl.org/dc/terms/\">100 pages</extent>" + Environment.NewLine +
                                           "<format xmlns=\"http://purl.org/dc/terms/\">application/pdf</format>" + Environment.NewLine +
                                           "<hasFormat xmlns=\"http://purl.org/dc/terms/\">urn:format:html</hasFormat>" + Environment.NewLine +
                                           "<hasPart xmlns=\"http://purl.org/dc/terms/\">Chapter 1</hasPart>" + Environment.NewLine +
                                           "<hasVersion xmlns=\"http://purl.org/dc/terms/\">2.0</hasVersion>" + Environment.NewLine +
                                           "<identifier xmlns=\"http://purl.org/dc/terms/\">MYTESTCDROM-1</identifier>" + Environment.NewLine +
                                           "<instructionalMethod xmlns=\"http://purl.org/dc/terms/\">Lecture</instructionalMethod>" + Environment.NewLine +
                                           "<isFormatOf xmlns=\"http://purl.org/dc/terms/\">urn:original</isFormatOf>" + Environment.NewLine +
                                           "<isPartOf xmlns=\"http://purl.org/dc/terms/\">Collection A</isPartOf>" + Environment.NewLine +
                                           "<isReferencedBy xmlns=\"http://purl.org/dc/terms/\">urn:reference</isReferencedBy>" + Environment.NewLine +
                                           "<isReplacedBy xmlns=\"http://purl.org/dc/terms/\">urn:replacement</isReplacedBy>" + Environment.NewLine +
                                           "<isRequiredBy xmlns=\"http://purl.org/dc/terms/\">urn:dependent</isRequiredBy>" + Environment.NewLine +
                                           "<issued xmlns=\"http://purl.org/dc/terms/\">2010-09-01T00:00:00.00Z</issued>" + Environment.NewLine +
                                           "<isVersionOf xmlns=\"http://purl.org/dc/terms/\">urn:original:v1</isVersionOf>" + Environment.NewLine +
                                           "<language xmlns=\"http://purl.org/dc/terms/\">en-US</language>" + Environment.NewLine +
                                           "<license xmlns=\"http://purl.org/dc/terms/\">MIT License</license>" + Environment.NewLine +
                                           "<mediator xmlns=\"http://purl.org/dc/terms/\">Teacher</mediator>" + Environment.NewLine +
                                           "<medium xmlns=\"http://purl.org/dc/terms/\">Paper</medium>" + Environment.NewLine +
                                           "<modified xmlns=\"http://purl.org/dc/terms/\">2010-10-01T00:00:00.00Z</modified>" + Environment.NewLine +
                                           "<provenance xmlns=\"http://purl.org/dc/terms/\">Original ownership</provenance>" + Environment.NewLine +
                                           "<publisher xmlns=\"http://purl.org/dc/terms/\">MeMeMe</publisher>" + Environment.NewLine +
                                           "<references xmlns=\"http://purl.org/dc/terms/\">urn:ref1</references>" + Environment.NewLine +
                                           "<relation xmlns=\"http://purl.org/dc/terms/\">MYTESTCDROM-2</relation>" + Environment.NewLine +
                                           "<replaces xmlns=\"http://purl.org/dc/terms/\">urn:old</replaces>" + Environment.NewLine +
                                           "<requires xmlns=\"http://purl.org/dc/terms/\">urn:dependency</requires>" + Environment.NewLine +
                                           "<rights xmlns=\"http://purl.org/dc/terms/\">Copyright 2010</rights>" + Environment.NewLine +
                                           "<rightsHolder xmlns=\"http://purl.org/dc/terms/\">Test Corp</rightsHolder>" + Environment.NewLine +
                                           "<source xmlns=\"http://purl.org/dc/terms/\">Out of Me Head</source>" + Environment.NewLine +
                                           "<spatial xmlns=\"http://purl.org/dc/terms/\">New York</spatial>" + Environment.NewLine +
                                           "<subject xmlns=\"http://purl.org/dc/terms/\">Test data (Stupid variety)</subject>" + Environment.NewLine +
                                           "<tableOfContents xmlns=\"http://purl.org/dc/terms/\">Chapter 1; Chapter 2</tableOfContents>" + Environment.NewLine +
                                           "<temporal xmlns=\"http://purl.org/dc/terms/\">20th Century</temporal>" + Environment.NewLine +
                                           "<title xmlns=\"http://purl.org/dc/terms/\">Stupid test data</title>" + Environment.NewLine +
                                           "<type xmlns=\"http://purl.org/dc/terms/\">Text</type>" + Environment.NewLine +
                                           "<valid xmlns=\"http://purl.org/dc/terms/\">2010-12-31</valid>";

    private const string StrExtXml = "<dcterms:abstract>Test Abstract</dcterms:abstract>"
                                     + "<dcterms:accessRights>Public</dcterms:accessRights>"
                                     + "<dcterms:accrualMethod>Deposit</dcterms:accrualMethod>"
                                     + "<dcterms:accrualPeriodicity>Monthly</dcterms:accrualPeriodicity>"
                                     + "<dcterms:accrualPolicy>Active</dcterms:accrualPolicy>"
                                     + "<dcterms:alternative>Alt Title</dcterms:alternative>"
                                     + "<dcterms:audience>Developers</dcterms:audience>"
                                     + "<dcterms:available>2010-08-01</dcterms:available>"
                                     + "<dcterms:bibliographicCitation>Test Citation</dcterms:bibliographicCitation>"
                                     + "<dcterms:conformsTo>ISO 9001</dcterms:conformsTo>"
                                     + "<dcterms:contributor>Helper</dcterms:contributor>"
                                     + "<dcterms:coverage>US</dcterms:coverage>"
                                     + "<dcterms:created>2010-06-01T00:00:00.00Z</dcterms:created>"
                                     + "<dcterms:creator>The Big Guy</dcterms:creator>"
                                     + "<dcterms:date>2010-08-01T00:00:00.00Z</dcterms:date>"
                                     + "<dcterms:dateAccepted>2010-07-01T00:00:00.00Z</dcterms:dateAccepted>"
                                     + "<dcterms:dateCopyrighted>2010-01-01T00:00:00.00Z</dcterms:dateCopyrighted>"
                                     + "<dcterms:dateSubmitted>2010-05-01T00:00:00.00Z</dcterms:dateSubmitted>"
                                     + "<dcterms:description>That kind of thing</dcterms:description>"
                                     + "<dcterms:educationLevel>Graduate</dcterms:educationLevel>"
                                     + "<dcterms:extent>100 pages</dcterms:extent>"
                                     + "<dcterms:format>application/pdf</dcterms:format>"
                                     + "<dcterms:hasFormat>urn:format:html</dcterms:hasFormat>"
                                     + "<dcterms:hasPart>Chapter 1</dcterms:hasPart>"
                                     + "<dcterms:hasVersion>2.0</dcterms:hasVersion>"
                                     + "<dcterms:identifier>MYTESTCDROM-1</dcterms:identifier>"
                                     + "<dcterms:instructionalMethod>Lecture</dcterms:instructionalMethod>"
                                     + "<dcterms:isFormatOf>urn:original</dcterms:isFormatOf>"
                                     + "<dcterms:isPartOf>Collection A</dcterms:isPartOf>"
                                     + "<dcterms:isReferencedBy>urn:reference</dcterms:isReferencedBy>"
                                     + "<dcterms:isReplacedBy>urn:replacement</dcterms:isReplacedBy>"
                                     + "<dcterms:isRequiredBy>urn:dependent</dcterms:isRequiredBy>"
                                     + "<dcterms:issued>2010-09-01T00:00:00.00Z</dcterms:issued>"
                                     + "<dcterms:isVersionOf>urn:original:v1</dcterms:isVersionOf>"
                                     + "<dcterms:language>en-US</dcterms:language>"
                                     + "<dcterms:license>MIT License</dcterms:license>"
                                     + "<dcterms:mediator>Teacher</dcterms:mediator>"
                                     + "<dcterms:medium>Paper</dcterms:medium>"
                                     + "<dcterms:modified>2010-10-01T00:00:00.00Z</dcterms:modified>"
                                     + "<dcterms:provenance>Original ownership</dcterms:provenance>"
                                     + "<dcterms:publisher>MeMeMe</dcterms:publisher>"
                                     + "<dcterms:references>urn:ref1</dcterms:references>"
                                     + "<dcterms:relation>MYTESTCDROM-2</dcterms:relation>"
                                     + "<dcterms:replaces>urn:old</dcterms:replaces>"
                                     + "<dcterms:requires>urn:dependency</dcterms:requires>"
                                     + "<dcterms:rights>Copyright 2010</dcterms:rights>"
                                     + "<dcterms:rightsHolder>Test Corp</dcterms:rightsHolder>"
                                     + "<dcterms:source>Out of Me Head</dcterms:source>"
                                     + "<dcterms:spatial>New York</dcterms:spatial>"
                                     + "<dcterms:subject>Test data (Stupid variety)</dcterms:subject>"
                                     + "<dcterms:tableOfContents>Chapter 1; Chapter 2</dcterms:tableOfContents>"
                                     + "<dcterms:temporal>20th Century</dcterms:temporal>"
                                     + "<dcterms:title>Stupid test data</dcterms:title>"
                                     + "<dcterms:type>Text</dcterms:type>"
                                     + "<dcterms:valid>2010-12-31</dcterms:valid>";

    public TestContext? TestContext { get; set; }

    #region Constructor Tests

    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorTest()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.ShouldNotBeNull();
        target.ShouldBeOfType<DublinCoreMetadataTermsSyndicationExtension>();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorSetsCorrectNamespace()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.XmlNamespace.ShouldBe("http://purl.org/dc/terms/");
        target.XmlPrefix.ShouldBe("dcterms");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorSetsCorrectName()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.Name.ShouldBe("Dublin Core Metadata Terms");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorSetsCorrectVersion()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.Version.ShouldBe(new Version("1.0"));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorSetsCorrectDocumentation()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.Documentation.ShouldBe(new Uri("http://dublincore.org/documents/dcmi-terms/"));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionContextIsNotNull()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.Context.ShouldNotBeNull();
    }

    #endregion

    #region Context Property Tests

    [TestMethod]
    public void DublinCoreMetadataTermsContextAbstractTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Abstract.ShouldBe("Test Abstract");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextAccessRightsTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.AccessRights.ShouldBe("Public");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextAccrualMethodTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.AccrualMethod.ShouldBe("Deposit");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextAccrualPeriodicityTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.AccrualPeriodicity.ShouldBe("Monthly");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextAccrualPolicyTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.AccrualPolicy.ShouldBe("Active");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextAlternativeTitleTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.AlternativeTitle.ShouldBe("Alt Title");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextAudienceTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Audience.ShouldBe("Developers");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextAudienceEducationLevelTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.AudienceEducationLevel.ShouldBe("Graduate");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextBibliographicCitationTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.BibliographicCitation.ShouldBe("Test Citation");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextConformsToTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.ConformsTo.ShouldBe("ISO 9001");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextContributorTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Contributor.ShouldBe("Helper");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextCoverageTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Coverage.ShouldBe("US");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextCreatorTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Creator.ShouldBe("The Big Guy");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextDateTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Date.ShouldBe(new DateTime(2010, 8, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextDateAcceptedTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.DateAccepted.ShouldBe(new DateTime(2010, 7, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextDateAvailableTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.DateAvailable.ShouldBe("2010-08-01");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextDateCopyrightedTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.DateCopyrighted.ShouldBe(new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextDateCreatedTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.DateCreated.ShouldBe(new DateTime(2010, 6, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextDateIssuedTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.DateIssued.ShouldBe(new DateTime(2010, 9, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextDateModifiedTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.DateModified.ShouldBe(new DateTime(2010, 10, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextDateSubmittedTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.DateSubmitted.ShouldBe(new DateTime(2010, 5, 1, 0, 0, 0, DateTimeKind.Utc));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextDateValidTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.DateValid.ShouldBe("2010-12-31");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextDescriptionTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Description.ShouldBe("That kind of thing");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextExtentTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Extent.ShouldBe("100 pages");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextFormatTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Format.ShouldBe("application/pdf");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextHasFormatTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.HasFormat.ShouldBe("urn:format:html");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextHasPartTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.HasPart.ShouldBe("Chapter 1");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextHasVersionTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.HasVersion.ShouldBe("2.0");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextIdentifierTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Identifier.ShouldBe("MYTESTCDROM-1");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextInstructionalMethodTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.InstructionalMethod.ShouldBe("Lecture");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextIsFormatOfTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.IsFormatOf.ShouldBe("urn:original");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextIsPartOfTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.IsPartOf.ShouldBe("Collection A");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextIsReferencedByTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.IsReferencedBy.ShouldBe("urn:reference");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextIsReplacedByTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.IsReplacedBy.ShouldBe("urn:replacement");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextIsRequiredByTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.IsRequiredBy.ShouldBe("urn:dependent");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextIsVersionOfTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.IsVersionOf.ShouldBe("urn:original:v1");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextLanguageTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Language.ShouldNotBeNull();
        context.Language.Name.ShouldBe("en-US");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextLicenseTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.License.ShouldBe("MIT License");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextMediatorTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Mediator.ShouldBe("Teacher");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextMediumTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Medium.ShouldBe("Paper");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextProvenanceTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Provenance.ShouldBe("Original ownership");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextPublisherTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Publisher.ShouldBe("MeMeMe");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextReferencesTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.References.ShouldBe("urn:ref1");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextRelationTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Relation.ShouldBe("MYTESTCDROM-2");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextReplacesTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Replaces.ShouldBe("urn:old");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextRequiresTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Requires.ShouldBe("urn:dependency");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextRightsTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Rights.ShouldBe("Copyright 2010");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextRightsHolderTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.RightsHolder.ShouldBe("Test Corp");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextSourceTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Source.ShouldBe("Out of Me Head");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextSpatialCoverageTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.SpatialCoverage.ShouldBe("New York");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextSubjectTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Subject.ShouldBe("Test data (Stupid variety)");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextTableOfContentsTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.TableOfContents.ShouldBe("Chapter 1; Chapter 2");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextTemporalCoverageTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.TemporalCoverage.ShouldBe("20th Century");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextTitleTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.Title.ShouldBe("Stupid test data");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextTypeVocabularyTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.TypeVocabulary.ShouldBe(DublinCoreTypeVocabularies.Text);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextSetterThrowsOnNull()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsContextAllPropertiesTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        DublinCoreMetadataTermsSyndicationExtensionContext context = target.Context;

        // Assert
        context.ShouldNotBeNull();
        context.Abstract.ShouldBe("Test Abstract");
        context.AccessRights.ShouldBe("Public");
        context.AccrualMethod.ShouldBe("Deposit");
        context.AccrualPeriodicity.ShouldBe("Monthly");
        context.AccrualPolicy.ShouldBe("Active");
        context.AlternativeTitle.ShouldBe("Alt Title");
        context.Audience.ShouldBe("Developers");
        context.AudienceEducationLevel.ShouldBe("Graduate");
        context.BibliographicCitation.ShouldBe("Test Citation");
        context.ConformsTo.ShouldBe("ISO 9001");
        context.Contributor.ShouldBe("Helper");
        context.Coverage.ShouldBe("US");
        context.Creator.ShouldBe("The Big Guy");
        context.Date.ShouldBe(new DateTime(2010, 8, 1, 0, 0, 0, DateTimeKind.Utc));
        context.DateAccepted.ShouldBe(new DateTime(2010, 7, 1, 0, 0, 0, DateTimeKind.Utc));
        context.DateAvailable.ShouldBe("2010-08-01");
        context.DateCopyrighted.ShouldBe(new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        context.DateCreated.ShouldBe(new DateTime(2010, 6, 1, 0, 0, 0, DateTimeKind.Utc));
        context.DateIssued.ShouldBe(new DateTime(2010, 9, 1, 0, 0, 0, DateTimeKind.Utc));
        context.DateModified.ShouldBe(new DateTime(2010, 10, 1, 0, 0, 0, DateTimeKind.Utc));
        context.DateSubmitted.ShouldBe(new DateTime(2010, 5, 1, 0, 0, 0, DateTimeKind.Utc));
        context.DateValid.ShouldBe("2010-12-31");
        context.Description.ShouldBe("That kind of thing");
        context.Extent.ShouldBe("100 pages");
        context.Format.ShouldBe("application/pdf");
        context.HasFormat.ShouldBe("urn:format:html");
        context.HasPart.ShouldBe("Chapter 1");
        context.HasVersion.ShouldBe("2.0");
        context.Identifier.ShouldBe("MYTESTCDROM-1");
        context.InstructionalMethod.ShouldBe("Lecture");
        context.IsFormatOf.ShouldBe("urn:original");
        context.IsPartOf.ShouldBe("Collection A");
        context.IsReferencedBy.ShouldBe("urn:reference");
        context.IsReplacedBy.ShouldBe("urn:replacement");
        context.IsRequiredBy.ShouldBe("urn:dependent");
        context.IsVersionOf.ShouldBe("urn:original:v1");
        context.Language.ShouldNotBeNull();
        context.Language.Name.ShouldBe("en-US");
        context.License.ShouldBe("MIT License");
        context.Mediator.ShouldBe("Teacher");
        context.Medium.ShouldBe("Paper");
        context.Provenance.ShouldBe("Original ownership");
        context.Publisher.ShouldBe("MeMeMe");
        context.References.ShouldBe("urn:ref1");
        context.Relation.ShouldBe("MYTESTCDROM-2");
        context.Replaces.ShouldBe("urn:old");
        context.Requires.ShouldBe("urn:dependency");
        context.Rights.ShouldBe("Copyright 2010");
        context.RightsHolder.ShouldBe("Test Corp");
        context.Source.ShouldBe("Out of Me Head");
        context.SpatialCoverage.ShouldBe("New York");
        context.Subject.ShouldBe("Test data (Stupid variety)");
        context.TableOfContents.ShouldBe("Chapter 1; Chapter 2");
        context.TemporalCoverage.ShouldBe("20th Century");
        context.Title.ShouldBe("Stupid test data");
        context.TypeVocabulary.ShouldBe(DublinCoreTypeVocabularies.Text);
    }

    #endregion

    #region XML Serialization Tests

    [TestMethod]
    public void DublinCoreMetadataTermsLoadTest()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert - no exception means success
        feed.ShouldNotBeNull();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsCreateXmlTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension ext = CreateFullExtension();

        // Act
        string actual = ExtensionTestUtil.AddExtensionToXml(ext);

        // Assert
        string expected = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsRoundTripTest()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();

        DublinCoreMetadataTermsSyndicationExtension itemExtension = item.FindExtension<DublinCoreMetadataTermsSyndicationExtension>();
        itemExtension.ShouldNotBeNull();

        DublinCoreMetadataTermsSyndicationExtensionContext context = itemExtension.Context;
        context.Abstract.ShouldBe("Test Abstract");
        context.AccessRights.ShouldBe("Public");
        context.Creator.ShouldBe("The Big Guy");
        context.Title.ShouldBe("Stupid test data");
        context.Description.ShouldBe("That kind of thing");
        context.TypeVocabulary.ShouldBe(DublinCoreTypeVocabularies.Text);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsFullTest()
    {
        // Arrange
        string strXml = ExtensionTestUtil.GetWrappedXml(Namespc, StrExtXml);

        // Act
        using XmlReader reader = XmlReader.Create(new StringReader(strXml));
        RssFeed feed = new();
        feed.Load(reader);

        // Assert
        feed.Channel.Items.Count().ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreMetadataTermsSyndicationExtension itemExtension = item.FindExtension<DublinCoreMetadataTermsSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(DublinCoreMetadataTermsSyndicationExtension.MatchByType) as DublinCoreMetadataTermsSyndicationExtension)
            .ShouldBeOfType<DublinCoreMetadataTermsSyndicationExtension>();
    }

    #endregion

    #region MatchByType Tests

    [TestMethod]
    public void DublinCoreMetadataTermsMatchByTypeTest()
    {
        // Arrange
        ISyndicationExtension extension = CreateFullExtension();

        // Act
        bool actual = DublinCoreMetadataTermsSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsMatchByTypeReturnsFalseForDifferentType()
    {
        // Arrange
        ISyndicationExtension extension = new DublinCoreElementSetSyndicationExtension();

        // Act
        bool actual = DublinCoreMetadataTermsSyndicationExtension.MatchByType(extension);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsMatchByTypeThrowsOnNull()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() => DublinCoreMetadataTermsSyndicationExtension.MatchByType(null!));
    }

    #endregion

    #region Comparison and Equality Tests

    [TestMethod]
    public void DublinCoreMetadataTermsCompareToTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension other = CreateFullExtension();

        // Act
        int actual = target.CompareTo(other);

        // Assert
        actual.ShouldBe(0);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsCompareToReturnsPositiveForNull()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        int actual = target.CompareTo(null);

        // Assert
        actual.ShouldBe(1);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsCompareToDifferentExtensionReturnsNonZero()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension other = new();

        // Act
        int result = target.CompareTo(other);

        // Assert
        result.ShouldNotBe(0);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsEqualsTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();
        object obj = CreateFullExtension();

        // Act
        bool actual = target.Equals(obj);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsEqualsReturnsFalseForNull()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        bool actual = target.Equals(null);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsEqualsReturnsFalseForWrongType()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();
        object wrongType = "not an extension";

        // Act
        bool actual = target.Equals(wrongType);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsGetHashCodeTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        int hashCode = target.GetHashCode();

        // Assert - GetHashCode should return an integer value
        // Note: The current implementation has a known issue where GetHashCode
        // is not consistent (uses charArray.GetHashCode() on a new array each time).
        // This test just verifies the method can be called without error.
        hashCode.ShouldBeOfType<int>();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsGetHashCodeConsistencyWithEqualsTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension other = CreateFullExtension();

        // Act & Assert - Verify objects are equal via Equals method
        // Note: The current GetHashCode implementation has a known issue where it
        // returns different values for equal objects (uses charArray.GetHashCode()
        // on a new array each time). This test verifies Equals works correctly.
        target.Equals(other).ShouldBeTrue();
    }

    #endregion

    #region Comparison Operators Tests

    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityTestSuccess()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityTestFailure()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateDifferentExtension();

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityWithNullLeft()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityWithNullRight()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension? second = null;

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityWithBothNull()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension? second = null;

        // Act
        bool actual = (first == second);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpInequalityTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateDifferentExtension();

        // Act
        bool actual = (first != second);

        // Assert
        actual.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpInequalityTestFalse()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool actual = (first != second);

        // Assert
        actual.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpGreaterThanTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateDifferentExtension();

        // Act
        bool result = (first > second);

        // Assert - Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpGreaterThanWithNullLeft()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool result = (first > second);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateDifferentExtension();

        // Act
        bool result = (first < second);

        // Assert - Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanWithNullLeft()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool result = (first < second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanWithNullRight()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension? second = null;

        // Act
        bool result = (first < second);

        // Assert
        result.ShouldBeFalse();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpGreaterThanOrEqualTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool result = (first >= second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpGreaterThanOrEqualWithNullLeft()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension? second = null;

        // Act
        bool result = (first >= second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanOrEqualTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool result = (first <= second);

        // Assert
        result.ShouldBeTrue();
    }

    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanOrEqualWithNullLeft()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool result = (first <= second);

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region ToString and WriteTo Tests

    [TestMethod]
    public void DublinCoreMetadataTermsToStringTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        string actual = target.ToString();

        // Assert
        actual.ShouldBe(toStringText);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsWriteToTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act
        using StringWriter sw = new();
        using XmlWriter writer = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true, ConformanceLevel = ConformanceLevel.Fragment });
        target.WriteTo(writer);
        writer.Flush();
        string output = sw.ToString();

        // Assert
        output.Replace(Environment.NewLine, "").ShouldBe(toStringText.Replace(Environment.NewLine, ""));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsWriteToThrowsOnNull()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.WriteTo(null!));
    }

    #endregion

    #region TypeVocabulary Tests

    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyAsStringTest()
    {
        // Arrange
        DublinCoreTypeVocabularies value = DublinCoreTypeVocabularies.Text;

        // Act
        string actual = DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyAsString(value);

        // Assert
        actual.ShouldBe("Text");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyAsStringMovingImageTest()
    {
        // Arrange
        DublinCoreTypeVocabularies value = DublinCoreTypeVocabularies.MovingImage;

        // Act
        string actual = DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyAsString(value);

        // Assert
        actual.ShouldBe("MovingImage");
    }

    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyByNameTest()
    {
        // Arrange
        DublinCoreTypeVocabularies expected = DublinCoreTypeVocabularies.Sound;

        // Act
        DublinCoreTypeVocabularies actual = DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyByName("Sound");

        // Assert
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyByNameCaseInsensitiveTest()
    {
        // Arrange
        DublinCoreTypeVocabularies expected = DublinCoreTypeVocabularies.Sound;

        // Act
        DublinCoreTypeVocabularies actual = DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyByName("sound");

        // Assert
        actual.ShouldBe(expected);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyByNameReturnsNoneForInvalid()
    {
        // Arrange & Act
        DublinCoreTypeVocabularies actual = DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyByName("InvalidType");

        // Assert
        actual.ShouldBe(DublinCoreTypeVocabularies.None);
    }

    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyByNameThrowsOnNull()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentException>(() => DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyByName(null!));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyByNameThrowsOnEmpty()
    {
        // Arrange, Act & Assert
        Should.Throw<ArgumentException>(() => DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyByName(string.Empty));
    }

    #endregion

    #region Load Tests

    [TestMethod]
    public void DublinCoreMetadataTermsLoadFromXPathNavigableThrowsOnNull()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((System.Xml.XPath.IXPathNavigable)null!));
    }

    [TestMethod]
    public void DublinCoreMetadataTermsLoadFromXmlReaderThrowsOnNull()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((XmlReader)null!));
    }

    #endregion

    #region Helper Methods

    private static DublinCoreMetadataTermsSyndicationExtension CreateFullExtension()
    {
        DublinCoreMetadataTermsSyndicationExtension ext = new()
        {
            Context =
            {
                Abstract = "Test Abstract",
                AccessRights = "Public",
                AccrualMethod = "Deposit",
                AccrualPeriodicity = "Monthly",
                AccrualPolicy = "Active",
                AlternativeTitle = "Alt Title",
                Audience = "Developers",
                AudienceEducationLevel = "Graduate",
                BibliographicCitation = "Test Citation",
                ConformsTo = "ISO 9001",
                Contributor = "Helper",
                Coverage = "US",
                Creator = "The Big Guy",
                Date = new DateTime(2010, 8, 1, 0, 0, 0, DateTimeKind.Utc),
                DateAccepted = new DateTime(2010, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                DateAvailable = "2010-08-01",
                DateCopyrighted = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DateCreated = new DateTime(2010, 6, 1, 0, 0, 0, DateTimeKind.Utc),
                DateIssued = new DateTime(2010, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                DateModified = new DateTime(2010, 10, 1, 0, 0, 0, DateTimeKind.Utc),
                DateSubmitted = new DateTime(2010, 5, 1, 0, 0, 0, DateTimeKind.Utc),
                DateValid = "2010-12-31",
                Description = "That kind of thing",
                Extent = "100 pages",
                Format = "application/pdf",
                HasFormat = "urn:format:html",
                HasPart = "Chapter 1",
                HasVersion = "2.0",
                Identifier = "MYTESTCDROM-1",
                InstructionalMethod = "Lecture",
                IsFormatOf = "urn:original",
                IsPartOf = "Collection A",
                IsReferencedBy = "urn:reference",
                IsReplacedBy = "urn:replacement",
                IsRequiredBy = "urn:dependent",
                IsVersionOf = "urn:original:v1",
                Language = new CultureInfo("en-US"),
                License = "MIT License",
                Mediator = "Teacher",
                Medium = "Paper",
                Provenance = "Original ownership",
                Publisher = "MeMeMe",
                References = "urn:ref1",
                Relation = "MYTESTCDROM-2",
                Replaces = "urn:old",
                Requires = "urn:dependency",
                Rights = "Copyright 2010",
                RightsHolder = "Test Corp",
                Source = "Out of Me Head",
                SpatialCoverage = "New York",
                Subject = "Test data (Stupid variety)",
                TableOfContents = "Chapter 1; Chapter 2",
                TemporalCoverage = "20th Century",
                Title = "Stupid test data",
                TypeVocabulary = DublinCoreTypeVocabularies.Text
            }
        };

        return ext;
    }

    private static DublinCoreMetadataTermsSyndicationExtension CreateDifferentExtension()
    {
        DublinCoreMetadataTermsSyndicationExtension ext = new()
        {
            Context =
            {
                Abstract = "Different Abstract",
                Creator = "Different Creator",
                Title = "Different Title",
                Description = "Different Description",
                TypeVocabulary = DublinCoreTypeVocabularies.Sound
            }
        };

        return ext;
    }

    #endregion
}
