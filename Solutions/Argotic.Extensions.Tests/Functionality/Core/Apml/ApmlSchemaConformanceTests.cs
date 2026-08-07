using System.Xml;
using System.Xml.Schema;

using Argotic.Syndication;

using Shouldly;

namespace Argotic.Extensions.Tests.Functionality.Core.Apml;

/// <summary>
/// Validates the documents this library writes against the official APML 0.6 XML Schema.
/// </summary>
/// <remarks>
///     <para>
///     Three earlier passes over this code recorded the APML 0.6 specification as archive-only and
///     unreachable, and every judgement about what APML requires was therefore made by reading Argotic and
///     guessing. The schema is not unreachable: it is published at
///     <a href="https://github.com/apml/spec-0.6">github.com/apml/spec-0.6</a> as <c>apml.xsd</c>, target
///     namespace <c>http://www.apml.org/apml-0.6</c> — the same namespace this library writes — and it is
///     reproduced verbatim below so that these tests read nothing from disk and reach no network.
///     </para>
///     <para>
///     Validating is the point. A string comparison asserts what someone believed the format requires; a
///     validating parser asserts what it actually requires, and it caught two conformance defects that no
///     amount of reading had found — fractional seconds on every date this library writes, and a
///     <c>Profile</c> missing the two child elements the schema makes mandatory.
///     </para>
/// </remarks>
[TestClass]
public class ApmlSchemaConformanceTests
{
    /// <summary>
    /// The APML 0.6 namespace, which is also the schema's target namespace.
    /// </summary>
    private const string ApmlNamespace = "http://www.apml.org/apml-0.6";

    /// <summary>
    /// The official APML 0.6 schema, reproduced verbatim from <c>github.com/apml/spec-0.6/apml.xsd</c>.
    /// </summary>
    private const string ApmlSchema = """
         <xs:schema
            xmlns:xs="http://www.w3.org/2001/XMLSchema"
            targetNamespace="http://www.apml.org/apml-0.6"
            xmlns:apml="http://www.apml.org/apml-0.6"
            elementFormDefault="qualified"
            attributeFormDefault="unqualified">

          <!--
            The root document type - a chunk containing nodes and links.

            Basic outline:
              <APML>
                <Head />
                <Body />
              </APML>
          -->
          <xs:element name="APML">
            <xs:complexType>
              <xs:sequence>
                <xs:element name="Head" type="apml:HeadType"/>
                <xs:element name="Body" type="apml:BodyType"/>
              </xs:sequence>

              <!--
                Defines the specification version this APML document currently adheres to.

                Currently expected to be 0.6.
              -->
              <xs:attribute name="version" type="xs:string" use="required"/>

              <!-- Allow additional attributes -->
            <xs:anyAttribute/>
            </xs:complexType>
          </xs:element>

          <!--
            The document header. Provides basic administrative data about the APML document.

            Basic outline:
              <Head>
                <Title/>
                <Generator/>
                <UserEmail/>
                <DateCreated/>
              </Head>
          -->
          <xs:complexType name="HeadType">
            <xs:sequence>
              <xs:element name="Title" type="xs:string"/>
              <xs:element name="Generator" type="xs:string"/>
              <xs:element name="UserEmail" type="apml:EmailType"/>
              <xs:element name="DateCreated" type="apml:ISO8601DateType"/>
            </xs:sequence>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            The document body. Contains the actual content of the user's APML data.

            Basic outline:
              <Body defaultprofile="">
                <Profile />
                <Profile />

                <Applications/>
              </Body>
          -->
          <xs:complexType name="BodyType">
            <xs:sequence>
              <!-- The list of profiles -->
              <xs:element name="Profile" type="apml:ProfileType" minOccurs="1" maxOccurs="unbounded"/>

              <!-- Provides application data storage -->
              <xs:element name="Applications" type="apml:ApplicationsBlockType" minOccurs="0" maxOccurs="1"/>
            </xs:sequence>

            <!--
              Specifies the default profile to use when either no user selection is made, or no user selection
              can be made.
            -->
            <xs:attribute name="defaultprofile" type="xs:string" use="required"/>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            Details of a profile.

            Basic outline:
              <Profile name="">
                <ImplicitData />
                <ExplicitData />
              </Profile>
          -->
          <xs:complexType name="ProfileType">
            <xs:sequence>
              <!-- Provides the implicit data associated with this profile -->
              <xs:element name="ImplicitData" type="apml:ImplicitBlockType"/>

              <!-- Provides the explicit data associated with this profile -->
              <xs:element name="ExplicitData" type="apml:ExplicitBlockType"/>
            </xs:sequence>

            <!-- Provides the name of the profile -->
            <xs:attribute name="name" type="xs:string" use="required"/>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            Details of implicit data - data that is automatically gathered.

            Basic outline:
              <ImplicitData>
                <Concepts/>
                <Sources/>
              </ImplicitData>
          -->
          <xs:complexType name="ImplicitBlockType">
            <xs:sequence>
              <xs:element name="Concepts" type="apml:ImplicitConceptsBlockType" minOccurs="0" maxOccurs="1"/>
              <xs:element name="Sources" type="apml:ImplicitSourcesBlockType" minOccurs="0" maxOccurs="1"/>
            </xs:sequence>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            Details of explicit data - data that the user provides.

            Basic outline:
              <ExplicitData>
                <Concepts/>
                <Sources/>
              </ExplicitData>
          -->
          <xs:complexType name="ExplicitBlockType">
            <xs:sequence>
              <xs:element name="Concepts" type="apml:ExplicitConceptsBlockType" minOccurs="0" maxOccurs="1"/>
              <xs:element name="Sources" type="apml:ExplicitSourcesBlockType" minOccurs="0" maxOccurs="1"/>
            </xs:sequence>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            Details of explit concepts and their scores

            Basic Outline:
              <Concepts>
                <Concept key="" value=""/>
              </Concepts>
          -->
          <xs:complexType name="ExplicitConceptsBlockType">
            <xs:sequence>
              <xs:element name="Concept" type="apml:ExplicitNodeType" minOccurs="0" maxOccurs="unbounded"/>
            </xs:sequence>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            Details of explicit sources and their scores

            Basic Outline:
              <Sources>
                <Source key="" value=""/>
              </Sources>
          -->
          <xs:complexType name="ExplicitSourcesBlockType">
            <xs:sequence>
              <xs:element name="Source" type="apml:ExplicitSourceType" minOccurs="0" maxOccurs="unbounded"/>
            </xs:sequence>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            Detail of explicit source and its authors.

            Basic Outline:
              <Source key="" name="" value="" type="">
              <Author key="" value=""/>
          -->
          <xs:complexType name="ExplicitSourceType">
            <xs:complexContent>
              <xs:extension base="apml:ExplicitNodeType">
                <xs:sequence>
                  <xs:element name="Author" type="apml:ExplicitNodeType" minOccurs="0" maxOccurs="unbounded"/>
                </xs:sequence>


                <!-- The (not necessarily unique) friendly name of the source -->
                <xs:attribute name="name" type="xs:string" use="required" />

                <!-- The type of the source. This is specified as a mime-type -->
                <xs:attribute name="type" type="apml:MimeType" use="required" />

                <!-- Allow additional attributes -->
                <xs:anyAttribute/>
              </xs:extension>
            </xs:complexContent>
          </xs:complexType>

          <!--
            Details of implicit concepts and their scores

            Basic Outline:
              <Concepts>
                <Concept key="" value="" from="" updated=""/>
              </Concepts>
          -->
          <xs:complexType name="ImplicitConceptsBlockType">
            <xs:sequence>
              <xs:element name="Concept" type="apml:ImplicitNodeType" minOccurs="0" maxOccurs="unbounded"/>
            </xs:sequence>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            Details of implicit sources and their scores

            Basic Outline:
              <Sources>
                <Source key="" value="" from="" updated=""/>
              </Sources>
          -->
          <xs:complexType name="ImplicitSourcesBlockType">
            <xs:sequence>
              <xs:element name="Source" type="apml:ImplicitSourceType" minOccurs="0" maxOccurs="unbounded"/>
            </xs:sequence>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            Detail of implicit source and its authors

            Basic Outline:
              <Source key="" name="" value="" type="">
                <Author key="" value="" from="" updated=""/>
              </Source>
          -->
          <xs:complexType name="ImplicitSourceType">
            <xs:complexContent>
              <xs:extension base="apml:ImplicitNodeType">
                <xs:sequence>
                  <xs:element name="Author" type="apml:ImplicitNodeType" minOccurs="0" maxOccurs="unbounded"/>
                </xs:sequence>

                <!-- The (not necessarily unique) friendly name of the source -->
                <xs:attribute name="name" type="xs:string" use="required" />

                <!-- The type of the source. This is specified as a mime-type -->
                <xs:attribute name="type" type="apml:MimeType" use="required" />

                <!-- Allow additional attributes -->
                <xs:anyAttribute/>
              </xs:extension>
            </xs:complexContent>
          </xs:complexType>

          <!--
            List of application specific data

            Basic Outline:
              <Applications>
                <Application name=""/>
              </Applications>
          -->
          <xs:complexType name="ApplicationsBlockType">
            <xs:sequence>
              <!-- List of application elements -->
              <xs:element name="Application" type="apml:ApplicationType" minOccurs="0" maxOccurs="unbounded"/>
            </xs:sequence>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            Storage element for application data

            Basic Outline:
              <Application name="">
                  Application Data goes here
              </Application>
          -->
          <xs:complexType name="ApplicationType">
            <xs:sequence>
              <xs:any namespace="##any" minOccurs="0" maxOccurs="unbounded" processContents="skip" />
            </xs:sequence>

            <!-- The unique name of the application -->
            <xs:attribute name="name" type="xs:string" use="required"/>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>


          <!--                   -->
          <!--    Base Types     -->
          <!--                   -->

          <!--
            Defines an Explicit Node - a node containing data explicitly specified by a user.
           -->
          <xs:complexType name="ExplicitNodeType">
            <!-- Provides the text or value of the node, such as a name or a term -->
            <xs:attribute name="key" type="xs:string" use="required"/>

            <!-- Provides the rank (or score) of the node -->
            <xs:attribute name="value" type="apml:NodeValueType" use="required"/>

            <!-- Allow additional attributes -->
            <xs:anyAttribute/>
          </xs:complexType>

          <!--
            Defines an Implicit Node - a node containing data automatically/programmatically discovered
            by an application
          -->
          <xs:complexType name="ImplicitNodeType">
            <xs:complexContent>
              <xs:extension base="apml:ExplicitNodeType">
                <!--
                  Contains a unique application identifier that allows each application to determine
                  the nodes that they have contributed. This, however, should not be used as a filter
                  when reading the data.
                -->
                <xs:attribute name="from" type="xs:string" use="required"/>

                <!--
                  A date indicating the last time this value was updated. Nodes older than 30 days
                  should be considered deprecated by other applications, and thus ignored.
                  This ensures that if an application that was contributing to an APML profile no longer
                  continues being used, it data will not result in the profile being stale.
                -->
                <xs:attribute name="updated" type="apml:ISO8601DateType" use="required"/>

                <!-- Allow additional attributes -->
                <xs:anyAttribute/>
              </xs:extension>
            </xs:complexContent>
          </xs:complexType>


          <!--                          -->
          <!--    Simple Data Types     -->
          <!--                          -->

          <!--
            The ISO8601-style date type. Accepted examples include:
              2007-03-09T22:35:56Z
            NOTE: Only a subset of ISO8601 formats are accepted.  In
                  particular, the time zone must be "Z"
          -->
          <xs:simpleType name="ISO8601DateType">
            <xs:restriction base="xs:dateTime">
              <!-- Restricts the dateTime format to only accept GMT times -->
              <xs:pattern value="[0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}Z"/>
            </xs:restriction>
          </xs:simpleType>

          <!--
            Defines an email address. This accepts any address in the format that contains one or more
            non-@ characters, then an @, then two or more dot-seperated blocks not containing @ symbols.
          -->
          <xs:simpleType name="EmailType">
            <xs:restriction base="xs:string">
              <xs:pattern value="[^@]+[@][^@]+(\.[^.]+)+" />
            </xs:restriction>
          </xs:simpleType>

          <!--
            The Node Rank Type. Defines the ways in which a rank can be specified.
            Accepted examples include:
              -0.5, 0, 0.5 (Bounded between -1 and 1)
          -->
          <xs:simpleType name="NodeValueType">
            <xs:restriction base="xs:decimal">
              <xs:minInclusive value="-1"/>
              <xs:maxInclusive value="1"/>
            </xs:restriction>
          </xs:simpleType>

          <!--
            Defines a mime-type. This accepts a string containing two parts divided by a '/'.
          -->
          <xs:simpleType name="MimeType">
            <xs:restriction base="xs:string">
              <xs:pattern value="[^/]+/[^/]+" />
            </xs:restriction>
          </xs:simpleType>
        </xs:schema>
        """;

    /// <summary>
    /// A fully populated document — every attribute the schema requires actually supplied — validates
    /// against the official schema with no errors at all.
    /// </summary>
    /// <remarks>
    ///     This is the test that fails the moment this library writes something APML does not permit. It was
    ///     red when written, on two counts that had nothing to do with each other.
    /// </remarks>
    [TestMethod]
    public void ASavedApmlDocument_ValidatesAgainstTheOfficialSchema()
    {
        string xml = Save(FullyPopulatedDocument());

        Validate(xml).ShouldBeEmpty();
    }

    /// <summary>
    /// A <c>Profile</c> carrying only explicit data still emits an empty <c>ImplicitData</c>, because the
    /// schema makes both child elements mandatory.
    /// </summary>
    /// <remarks>
    ///     An empty container is an honest statement: it says "no implicit data" and asserts nothing that is
    ///     untrue. That is what separates it from the <c>value</c> question, where every available spelling
    ///     either fabricates a score or omits a required attribute.
    /// </remarks>
    [TestMethod]
    public void AProfileWithOnlyExplicitData_StillEmitsAnEmptyImplicitDataElement()
    {
        string xml = Save(ExplicitOnlyDocument());

        xml.ShouldContain("<ImplicitData", Case.Sensitive);
        xml.ShouldContain("<ExplicitData", Case.Sensitive);
        Validate(xml).ShouldBeEmpty();
    }

    /// <summary>
    /// Dates are written without fractional seconds, which the schema's date type forbids.
    /// </summary>
    [TestMethod]
    public void ASavedApmlDocument_WritesDatesWithoutFractionalSeconds()
    {
        string xml = Save(FullyPopulatedDocument());

        xml.ShouldContain("<DateCreated>2024-01-01T00:00:00Z</DateCreated>", Case.Sensitive);
        xml.ShouldContain("updated=\"2024-02-03T04:05:06Z\"", Case.Sensitive);
        xml.ShouldNotContain(".00Z", Case.Sensitive);
    }

    /// <summary>
    /// A local-kind date is written as an instant in UTC with a literal <c>Z</c>, never as a numeric offset.
    /// </summary>
    /// <remarks>
    ///     This assertion survives the container's clock. <c>SyndicationDateTimeUtility.ToRfc3339DateTime</c>
    ///     formats a local-kind value with <c>zzz</c>, which renders <c>+00:00</c> even where the offset is
    ///     zero — so the shape differs from <c>Z</c> whatever the machine's timezone, and this test cannot
    ///     pass vacuously on a UTC build agent.
    /// </remarks>
    [TestMethod]
    public void ALocalKindDate_IsWrittenAsUtcWithALiteralZ()
    {
        ApmlDocument document = ExplicitOnlyDocument();
        document.Head.CreatedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Local);

        string xml = Save(document);

        xml.ShouldNotContain("+00:00", Case.Sensitive);
        xml.ShouldNotContain("-00:00", Case.Sensitive);
        xml.ShouldContain("Z</DateCreated>", Case.Sensitive);
        Validate(xml).ShouldBeEmpty();
    }

    /// <summary>
    /// The one deviation this library takes deliberately: a concept whose score was never assigned omits
    /// the required <c>value</c> attribute, and that is the <i>only</i> thing wrong with the document.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     APML declares <c>value</c> <c>use="required"</c>, typed <c>NodeValueType</c> — an
    ///     <c>xs:decimal</c> restricted to <c>[-1, 1]</c>, which has no spelling for "unknown". A score that
    ///     was never established is therefore not representable in conformant APML, and the only question is
    ///     which non-conformance to prefer. Omission is preferred over <c>0.00</c>: a document that fails
    ///     validation announces its own incompleteness, whereas one that validates while asserting a neutral
    ///     interest nobody expressed is a lie that passes review.
    ///     </para>
    ///     <para>
    ///     Asserting the exact count, and the exact attribute, is what keeps that a <i>known</i> deviation
    ///     rather than a general tolerance for invalid output. If this library starts emitting some other
    ///     invalid construct, this test fails.
    ///     </para>
    /// </remarks>
    [TestMethod]
    public void AnUnscoredConcept_LeavesTheMissingValueAsTheOnlySchemaViolation()
    {
        ApmlDocument document = ExplicitOnlyDocument();
        document.Profiles[0].ExplicitConcepts.Add(new ApmlConcept { Key = "unscored" });

        List<string> errors = Validate(Save(document));

        errors.Count.ShouldBe(1);
        errors[0].ShouldContain("value", Case.Sensitive);
        errors[0].ShouldContain("required", Case.Sensitive);
    }

    /// <summary>
    /// A schema-valid document read and written back out is still schema-valid.
    /// </summary>
    /// <remarks>
    ///     This is the assertion that matters most, and the one that was failing. The document below is
    ///     conformant on the way in — it is the shape of the repository's own
    ///     <c>SampleData/ApmlDocument.xml</c> — and every date in it is written back with a hundredth of a
    ///     second appended, so before the APML-local date formatter existed, reading and re-saving <i>any</i>
    ///     real APML document produced an invalid one. Nothing about the round trip was lossy in the object
    ///     model, which is exactly why reading the code had never found it.
    /// </remarks>
    [TestMethod]
    public void AValidDocumentReadAndWrittenBack_IsStillValid()
    {
        const string Original = """
            <?xml version="1.0" encoding="utf-8"?>
            <APML xmlns="http://www.apml.org/apml-0.6" version="0.6">
              <Head>
                <Title>Sample APML Document</Title>
                <Generator>Argotic</Generator>
                <UserEmail>user@example.com</UserEmail>
                <DateCreated>2024-01-01T08:00:00Z</DateCreated>
              </Head>
              <Body defaultprofile="work">
                <Profile name="work">
                  <ImplicitData>
                    <Concepts>
                      <Concept key="technology" value="0.95" from="https://news.example.com" updated="2024-01-01T12:00:00Z" />
                    </Concepts>
                    <Sources>
                      <Source key="https://example.com/feed" value="0.98" name="Example Blog" type="application/rss+xml" from="https://example.com" updated="2024-01-01T12:00:00Z" />
                    </Sources>
                  </ImplicitData>
                  <ExplicitData>
                    <Concepts>
                      <Concept key="programming" value="1.00" />
                    </Concepts>
                  </ExplicitData>
                </Profile>
              </Body>
            </APML>
            """;

        Validate(Original).ShouldBeEmpty("the fixture must be valid for the round trip to mean anything");

        ApmlDocument document = new();
        using (MemoryStream input = new(System.Text.Encoding.UTF8.GetBytes(Original)))
        {
            document.Load(input);
        }

        string roundTripped = Save(document);

        Validate(roundTripped).ShouldBeEmpty();
        roundTripped.ShouldNotContain(".00Z", Case.Sensitive);
    }

    /// <summary>
    /// Builds a document in which every attribute and element the schema requires is supplied, across both
    /// the implicit and the explicit halves of a profile.
    /// </summary>
    /// <remarks>
    ///     The two halves are populated differently on purpose, because the schema types them differently.
    ///     An explicit node is an <c>ExplicitNodeType</c> and carries <c>key</c> and <c>value</c> only; an
    ///     implicit node is an <c>ImplicitNodeType</c>, which adds <c>from</c> and <c>updated</c> and makes
    ///     both of them <c>use="required"</c>. Argotic models both with a single pair of classes and writes
    ///     <c>from</c>/<c>updated</c> whenever they happen to be set, so putting an annotated entity in the
    ///     explicit collection produces attributes the explicit type does not declare.
    /// </remarks>
    /// <returns>The document.</returns>
    private static ApmlDocument FullyPopulatedDocument()
    {
        DateTime updated = new(2024, 2, 3, 4, 5, 6, DateTimeKind.Utc);

        ApmlDocument document = new() { DefaultProfileName = "Home" };
        document.Head.Title = "Attention profile";
        document.Head.Generator = "Argotic";
        document.Head.EmailAddress = "ada@example.com";
        document.Head.CreatedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        ApmlProfile profile = new() { Name = "Home" };

        // Explicit: stated by the user, so key and value only.
        profile.ExplicitConcepts.Add(new ApmlConcept("syndication", 0.9m));
        ApmlSource explicitSource = new("http://example.com/feed", "Example", "application/rss+xml", 0.8m);
        explicitSource.Authors.Add(new ApmlAuthor("ada", 0.7m));
        profile.ExplicitSources.Add(explicitSource);

        // Implicit: inferred by a machine, so from and updated are required as well.
        profile.ImplicitConcepts.Add(new ApmlConcept("dotnet", 0.6m, "Argotic", updated));
        ApmlSource implicitSource = new("http://example.com/other", "Other", "application/atom+xml", 0.5m, "Argotic", updated);
        implicitSource.Authors.Add(new ApmlAuthor("grace", 0.4m, "Argotic", updated));
        profile.ImplicitSources.Add(implicitSource);

        document.Profiles.Add(profile);
        return document;
    }

    /// <summary>
    /// Builds a document whose single profile holds explicit data and nothing implicit.
    /// </summary>
    /// <returns>The document.</returns>
    private static ApmlDocument ExplicitOnlyDocument()
    {
        ApmlDocument document = new() { DefaultProfileName = "Home" };
        document.Head.Title = "Attention profile";
        document.Head.Generator = "Argotic";
        document.Head.EmailAddress = "ada@example.com";
        document.Head.CreatedOn = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        ApmlProfile profile = new() { Name = "Home" };
        profile.ExplicitConcepts.Add(new ApmlConcept("syndication", 0.9m));
        document.Profiles.Add(profile);

        return document;
    }

    /// <summary>
    /// Saves a document to a string.
    /// </summary>
    /// <param name="document">The document to save.</param>
    /// <returns>The saved document text.</returns>
    private static string Save(ApmlDocument document)
    {
        using MemoryStream stream = new();
        document.Save(stream);
        stream.Position = 0;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Validates the supplied document against the official APML 0.6 schema.
    /// </summary>
    /// <param name="xml">The document to validate.</param>
    /// <returns>The validation messages, empty when the document is valid.</returns>
    private static List<string> Validate(string xml)
    {
        XmlSchemaSet schemas = new();
        using (StringReader schemaReader = new(ApmlSchema))
        {
            using XmlReader schemaXmlReader = XmlReader.Create(schemaReader);
            schemas.Add(ApmlNamespace, schemaXmlReader);
        }

        schemas.Compile();

        List<string> errors = [];
        XmlReaderSettings settings = new() { ValidationType = ValidationType.Schema, Schemas = schemas };
        settings.ValidationEventHandler += (_, e) => errors.Add(e.Message);

        using StringReader documentReader = new(xml);
        using XmlReader reader = XmlReader.Create(documentReader, settings);
        while (reader.Read())
        {
        }

        return errors;
    }
}