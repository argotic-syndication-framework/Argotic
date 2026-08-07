using System.Globalization;
using System.Xml;
using Argotic.Extensions.Core;
using Argotic.Syndication;
using Shouldly;
using static Argotic.Common.ComparisonOperatorExtensions;

namespace Argotic.Extensions.Tests.Functionality.Core.DublinCoreMetadataTerms;

/// <summary>
/// Covers the Dublin Core Metadata Terms extension — the fifty-five <c>dcterms:</c> elements under
/// <c>http://purl.org/dc/terms/</c> — from the context that holds them, through the XML
/// <c>WriteTo</c> and <c>ToString</c> produce, to the type vocabulary lookups and the comparison and
/// equality contracts.
/// </summary>
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

    /// <summary>
    /// The parameterless constructor yields an instance of the Dublin Core metadata terms extension type.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorTest()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.ShouldNotBeNull();
        target.ShouldBeOfType<DublinCoreMetadataTermsSyndicationExtension>();
    }

    /// <summary>
    /// A new extension declares the terms namespace <c>http://purl.org/dc/terms/</c> under the prefix <c>dcterms</c>, not the older element-set namespace.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorSetsCorrectNamespace()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.XmlNamespace.ShouldBe("http://purl.org/dc/terms/");
        target.XmlPrefix.ShouldBe("dcterms");
    }

    /// <summary>
    /// A new extension names itself <c>Dublin Core Metadata Terms</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorSetsCorrectName()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.Name.ShouldBe("Dublin Core Metadata Terms");
    }

    /// <summary>
    /// A new extension reports version <c>1.0</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorSetsCorrectVersion()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.Version.ShouldBe(new Version("1.0"));
    }

    /// <summary>
    /// A new extension points its documentation at <c>http://dublincore.org/documents/dcmi-terms/</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsSyndicationExtensionConstructorSetsCorrectDocumentation()
    {
        // Arrange & Act
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Assert
        target.Documentation.ShouldBe(new Uri("http://dublincore.org/documents/dcmi-terms/"));
    }

    /// <summary>
    /// A new extension already holds a context; reading it never gives <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// The abstract set on the fixture reaches the context as <c>Test Abstract</c>.
    /// </summary>
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

    /// <summary>
    /// The access rights set on the fixture reach the context as <c>Public</c>.
    /// </summary>
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

    /// <summary>
    /// The accrual method set on the fixture reaches the context as <c>Deposit</c>.
    /// </summary>
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

    /// <summary>
    /// The accrual periodicity set on the fixture reaches the context as <c>Monthly</c>.
    /// </summary>
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

    /// <summary>
    /// The accrual policy set on the fixture reaches the context as <c>Active</c>.
    /// </summary>
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

    /// <summary>
    /// The alternative title set on the fixture reaches the context as <c>Alt Title</c>.
    /// </summary>
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

    /// <summary>
    /// The audience set on the fixture reaches the context as <c>Developers</c>.
    /// </summary>
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

    /// <summary>
    /// The audience education level set on the fixture reaches the context as <c>Graduate</c>.
    /// </summary>
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

    /// <summary>
    /// The bibliographic citation set on the fixture reaches the context as <c>Test Citation</c>.
    /// </summary>
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

    /// <summary>
    /// The conformance statement set on the fixture reaches the context as <c>ISO 9001</c>.
    /// </summary>
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

    /// <summary>
    /// The contributor set on the fixture reaches the context as <c>Helper</c>.
    /// </summary>
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

    /// <summary>
    /// The coverage set on the fixture reaches the context as <c>US</c>.
    /// </summary>
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

    /// <summary>
    /// The creator set on the fixture reaches the context as <c>The Big Guy</c>.
    /// </summary>
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

    /// <summary>
    /// The date set on the fixture reaches the context as 1 August 2010, kind included.
    /// </summary>
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

    /// <summary>
    /// The acceptance date set on the fixture reaches the context as 1 July 2010, kind included.
    /// </summary>
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

    /// <summary>
    /// The availability date reaches the context as the string <c>2010-08-01</c>; unlike its sibling terms, <c>DateAvailable</c> is untyped.
    /// </summary>
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

    /// <summary>
    /// The copyright date set on the fixture reaches the context as 1 January 2010, kind included.
    /// </summary>
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

    /// <summary>
    /// The creation date set on the fixture reaches the context as 1 June 2010, kind included.
    /// </summary>
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

    /// <summary>
    /// The issue date set on the fixture reaches the context as 1 September 2010, kind included.
    /// </summary>
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

    /// <summary>
    /// The modification date set on the fixture reaches the context as 1 October 2010, kind included.
    /// </summary>
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

    /// <summary>
    /// The submission date set on the fixture reaches the context as 1 May 2010, kind included.
    /// </summary>
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

    /// <summary>
    /// The validity date reaches the context as the string <c>2010-12-31</c>; unlike its sibling terms, <c>DateValid</c> is untyped.
    /// </summary>
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

    /// <summary>
    /// The description set on the fixture reaches the context as <c>That kind of thing</c>.
    /// </summary>
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

    /// <summary>
    /// The extent set on the fixture reaches the context as <c>100 pages</c>.
    /// </summary>
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

    /// <summary>
    /// The format set on the fixture reaches the context as <c>application/pdf</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>hasFormat</c> relation set on the fixture reaches the context as <c>urn:format:html</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>hasPart</c> relation set on the fixture reaches the context as <c>Chapter 1</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>hasVersion</c> relation set on the fixture reaches the context as <c>2.0</c>.
    /// </summary>
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

    /// <summary>
    /// The identifier set on the fixture reaches the context as <c>MYTESTCDROM-1</c>.
    /// </summary>
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

    /// <summary>
    /// The instructional method set on the fixture reaches the context as <c>Lecture</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>isFormatOf</c> relation set on the fixture reaches the context as <c>urn:original</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>isPartOf</c> relation set on the fixture reaches the context as <c>Collection A</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>isReferencedBy</c> relation set on the fixture reaches the context as <c>urn:reference</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>isReplacedBy</c> relation set on the fixture reaches the context as <c>urn:replacement</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>isRequiredBy</c> relation set on the fixture reaches the context as <c>urn:dependent</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>isVersionOf</c> relation set on the fixture reaches the context as <c>urn:original:v1</c>.
    /// </summary>
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

    /// <summary>
    /// The language set on the fixture reaches the context as a culture named <c>en-US</c>.
    /// </summary>
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

    /// <summary>
    /// The license set on the fixture reaches the context as <c>MIT License</c>.
    /// </summary>
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

    /// <summary>
    /// The mediator set on the fixture reaches the context as <c>Teacher</c>.
    /// </summary>
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

    /// <summary>
    /// The medium set on the fixture reaches the context as <c>Paper</c>.
    /// </summary>
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

    /// <summary>
    /// The provenance set on the fixture reaches the context as <c>Original ownership</c>.
    /// </summary>
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

    /// <summary>
    /// The publisher set on the fixture reaches the context as <c>MeMeMe</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>references</c> relation set on the fixture reaches the context as <c>urn:ref1</c>.
    /// </summary>
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

    /// <summary>
    /// The relation set on the fixture reaches the context as <c>MYTESTCDROM-2</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>replaces</c> relation set on the fixture reaches the context as <c>urn:old</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>requires</c> relation set on the fixture reaches the context as <c>urn:dependency</c>.
    /// </summary>
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

    /// <summary>
    /// The rights statement set on the fixture reaches the context as <c>Copyright 2010</c>.
    /// </summary>
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

    /// <summary>
    /// The rights holder set on the fixture reaches the context as <c>Test Corp</c>.
    /// </summary>
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

    /// <summary>
    /// The source set on the fixture reaches the context as <c>Out of Me Head</c>.
    /// </summary>
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

    /// <summary>
    /// The spatial coverage set on the fixture reaches the context as <c>New York</c>.
    /// </summary>
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

    /// <summary>
    /// The subject set on the fixture reaches the context as <c>Test data (Stupid variety)</c>.
    /// </summary>
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

    /// <summary>
    /// The table of contents set on the fixture reaches the context as <c>Chapter 1; Chapter 2</c>.
    /// </summary>
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

    /// <summary>
    /// The temporal coverage set on the fixture reaches the context as <c>20th Century</c>.
    /// </summary>
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

    /// <summary>
    /// The title set on the fixture reaches the context as <c>Stupid test data</c>.
    /// </summary>
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

    /// <summary>
    /// The type vocabulary set on the fixture reaches the context as <c>Text</c>.
    /// </summary>
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

    /// <summary>
    /// Assigning a <see langword="null"/> context throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsContextSetterThrowsOnNull()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Context = null!);
    }

    /// <summary>
    /// Every one of the fifty-five terms the fixture populates survives the object initializer intact, read back in one pass.
    /// </summary>
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

    /// <summary>
    /// An RSS 2.0 feed carrying all fifty-five <c>dcterms:</c> elements parses without throwing.
    /// </summary>
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

    /// <summary>
    /// Attaching the extension to an RSS item emits every populated term under the <c>dcterms</c> prefix, in the order the fixture spells them.
    /// </summary>
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

    /// <summary>
    /// Terms written into a feed come back off the parsed item unchanged, spot-checked from the abstract through to the <c>Text</c> type vocabulary.
    /// </summary>
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
        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();

        DublinCoreMetadataTermsSyndicationExtension? itemExtension = item.FindExtension<DublinCoreMetadataTermsSyndicationExtension>();
        itemExtension.ShouldNotBeNull();

        DublinCoreMetadataTermsSyndicationExtensionContext context = itemExtension.Context;
        context.Abstract.ShouldBe("Test Abstract");
        context.AccessRights.ShouldBe("Public");
        context.Creator.ShouldBe("The Big Guy");
        context.Title.ShouldBe("Stupid test data");
        context.Description.ShouldBe("That kind of thing");
        context.TypeVocabulary.ShouldBe(DublinCoreTypeVocabularies.Text);
    }

    /// <summary>
    /// An item parsed from a feed carrying the <c>dcterms:</c> elements exposes the extension both by generic lookup and through <c>MatchByType</c>.
    /// </summary>
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
        feed.Channel.Items.Count.ShouldBe(1);
        RssItem item = feed.Channel.Items.Single();
        item.HasExtensions.ShouldBeTrue();
        DublinCoreMetadataTermsSyndicationExtension? itemExtension = item.FindExtension<DublinCoreMetadataTermsSyndicationExtension>();
        itemExtension.ShouldNotBeNull();
        (item.FindExtension(DublinCoreMetadataTermsSyndicationExtension.MatchByType) as DublinCoreMetadataTermsSyndicationExtension)
            .ShouldBeOfType<DublinCoreMetadataTermsSyndicationExtension>();
    }

    #endregion

    #region MatchByType Tests

    /// <summary>
    /// <c>MatchByType</c> accepts an instance of its own extension type.
    /// </summary>
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

    /// <summary>
    /// <c>MatchByType</c> rejects the Dublin Core element set extension, which is a different type in the same family.
    /// </summary>
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

    /// <summary>
    /// <c>MatchByType</c> throws <see cref="ArgumentNullException"/> rather than returning <see langword="false"/> for a <see langword="null"/> extension.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsMatchByTypeThrowsOnNull() =>
        // Arrange, Act & Assert
        Should.Throw<ArgumentNullException>(() => DublinCoreMetadataTermsSyndicationExtension.MatchByType(null!));

    #endregion

    #region Comparison and Equality Tests

    /// <summary>
    /// Two extensions holding identical context compare equal.
    /// </summary>
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

    /// <summary>
    /// Comparing against <see langword="null"/> returns <c>1</c>, sorting every instance after nothing.
    /// </summary>
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

    /// <summary>
    /// A populated extension does not compare equal to an empty one.
    /// </summary>
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

    /// <summary>
    /// An extension is equal to a separately constructed extension holding the same context.
    /// </summary>
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

    /// <summary>
    /// An extension is unequal to <see langword="null"/>.
    /// </summary>
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

    /// <summary>
    /// An extension is unequal to an object of an unrelated type, here a string.
    /// </summary>
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

    /// <summary>
    /// A hash code is stable across repeated calls on the same instance.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsGetHashCodeTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();

        // Act & Assert - the hash is content-based, so it is stable across calls
        target.GetHashCode().ShouldBe(target.GetHashCode());
    }

    /// <summary>
    /// Equal extensions hash equally, which is what the equality contract requires.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsGetHashCodeConsistencyWithEqualsTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension other = CreateFullExtension();

        // Act & Assert - equal objects must hash equally, which is what this test's name promises
        target.Equals(other).ShouldBeTrue();
        target.GetHashCode().ShouldBe(other.GetHashCode());
    }

    #endregion

    #region Comparison Operators Tests

    /// <summary>
    /// Extensions holding identical context are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityTestSuccess()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Extensions differing in abstract, creator, title, description and type vocabulary are not equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityTestFailure()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateDifferentExtension();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// A <see langword="null"/> left operand is not equal to an instance.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityWithNullLeft()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// An instance is not equal to a <see langword="null"/> right operand.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityWithNullRight()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension? second = null;

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Two <see langword="null"/> references are equal under <c>==</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpEqualityWithBothNull()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension? second = null;

        // Act
        bool actual = first == second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Extensions holding different context are unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpInequalityTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateDifferentExtension();

        // Act
        bool actual = first != second;

        // Assert
        actual.ShouldBeTrue();
    }

    /// <summary>
    /// Extensions holding identical context are not unequal under <c>!=</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpInequalityTestFalse()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool actual = first != second;

        // Assert
        actual.ShouldBeFalse();
    }

    /// <summary>
    /// Comparing two differing extensions with <c>&gt;</c> completes and yields a boolean; the test pins no direction.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpGreaterThanTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateDifferentExtension();

        // Act
        bool result = first > second;

        // Assert - Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    /// <summary>
    /// <see langword="null"/> is not greater than any instance.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpGreaterThanWithNullLeft()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool result = first > second;

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// Comparing two differing extensions with <c>&lt;</c> completes and yields a boolean; the test pins no direction.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateDifferentExtension();

        // Act
        bool result = first < second;

        // Assert - Just verify the operator works without throwing
        result.ShouldBeOneOf(true, false);
    }

    /// <summary>
    /// <see langword="null"/> is less than any instance.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanWithNullLeft()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// No instance is less than <see langword="null"/>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanWithNullRight()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension? second = null;

        // Act
        bool result = first < second;

        // Assert
        result.ShouldBeFalse();
    }

    /// <summary>
    /// Extensions holding identical context satisfy <c>&gt;=</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpGreaterThanOrEqualTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// <see langword="null"/> is greater than or equal to <see langword="null"/> — both operands here are <see langword="null"/>, not just the left one.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpGreaterThanOrEqualWithNullLeft()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension? second = null;

        // Act
        bool result = first >= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// Extensions holding identical context satisfy <c>&lt;=</c>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanOrEqualTest()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension first = CreateFullExtension();
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    /// <summary>
    /// <see langword="null"/> is less than or equal to any instance.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsOpLessThanOrEqualWithNullLeft()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension? first = null;
        DublinCoreMetadataTermsSyndicationExtension second = CreateFullExtension();

        // Act
        bool result = first <= second;

        // Assert
        result.ShouldBeTrue();
    }

    #endregion

    #region ToString and WriteTo Tests

    /// <summary>
    /// <c>ToString</c> renders each populated term on its own line, every one redeclaring the terms namespace as its default.
    /// </summary>
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

    /// <summary>
    /// Writing to an <see cref="XmlWriter"/> emits the same terms as <c>ToString</c>, once line breaks are discounted.
    /// </summary>
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
        output.Replace(Environment.NewLine, "", StringComparison.Ordinal).ShouldBe(toStringText.Replace(Environment.NewLine, "", StringComparison.Ordinal));
    }

    /// <summary>
    /// Writing to a <see langword="null"/> writer throws <see cref="ArgumentNullException"/>.
    /// </summary>
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

    /// <summary>
    /// The <c>Text</c> vocabulary term renders as the string <c>Text</c>.
    /// </summary>
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

    /// <summary>
    /// The <c>MovingImage</c> term keeps its camel case rather than being split or lowercased.
    /// </summary>
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

    /// <summary>
    /// The name <c>Sound</c> resolves back to the <c>Sound</c> vocabulary term.
    /// </summary>
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

    /// <summary>
    /// Lookup ignores case, so <c>sound</c> resolves to the <c>Sound</c> term.
    /// </summary>
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

    /// <summary>
    /// An unrecognised name resolves to <c>None</c> rather than throwing.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyByNameReturnsNoneForInvalid()
    {
        // Arrange & Act
        DublinCoreTypeVocabularies actual = DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyByName("InvalidType");

        // Assert
        actual.ShouldBe(DublinCoreTypeVocabularies.None);
    }

    /// <summary>
    /// A <see langword="null"/> name resolves to <c>None</c> rather than throwing.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyByNameReturnsNoneOnNull() =>
        // Arrange, Act & Assert
        DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyByName(null!).ShouldBe(DublinCoreTypeVocabularies.None);

    /// <summary>
    /// An empty name resolves to <c>None</c> rather than throwing.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsTypeVocabularyByNameReturnsNoneOnEmpty() =>
        // Arrange, Act & Assert
        DublinCoreMetadataTermsSyndicationExtension.TypeVocabularyByName(string.Empty).ShouldBe(DublinCoreTypeVocabularies.None);

    #endregion

    #region Load Tests

    /// <summary>
    /// Loading from a <see langword="null"/> <c>IXPathNavigable</c> throws <see cref="ArgumentNullException"/>.
    /// </summary>
    [TestMethod]
    public void DublinCoreMetadataTermsLoadFromXPathNavigableThrowsOnNull()
    {
        // Arrange
        DublinCoreMetadataTermsSyndicationExtension target = new();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => target.Load((System.Xml.XPath.IXPathNavigable)null!));
    }

    /// <summary>
    /// Loading from a <see langword="null"/> <see cref="XmlReader"/> throws <see cref="ArgumentNullException"/>.
    /// </summary>
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