/****************************************************************************
Modification History:
*****************************************************************************
Date		Author		Description
*****************************************************************************
01/30/2008	brian.kuhn	Created DublinCoreTypeVocabularies Enumeration
****************************************************************************/
using System;

using Argotic.Common;

namespace Argotic.Extensions.Core
{
    /// <summary>
    /// Represents a general, cross-domain list of approved terms that may be used as values when identifying the nature or genre of a resource.
    /// </summary>
    /// <seealso cref="DublinCoreElementSetSyndicationExtension"/>
    /// <seealso cref="DublinCoreMetadataTermsSyndicationExtension"/>
    /// <remarks>
    ///     For more information about the DCMI Type Vocabulary, see <a href="http://dublincore.org/documents/dcmi-type-vocabulary/">http://dublincore.org/documents/dcmi-type-vocabulary/</a>.
    /// </remarks>
    [Serializable()]
    [Flags()]
    public enum DublinCoreTypeVocabularies
    {
        /// <summary>
        /// No type vocabulary specified.
        /// </summary>
        [EnumerationMetadata(DisplayName = "", AlternateValue = "")]
        None                = 0,

        /// <summary>
        /// The type represents an aggregation of resources.
        /// </summary>
        /// <remarks>
        ///     A collection is described as a group; its parts may also be separately described.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Collection", AlternateValue = "Collection")]
        Collection          = 1,

        /// <summary>
        /// The type represents data encoded in a defined structure.
        /// </summary>
        /// <remarks>
        ///     Examples include lists, tables, and databases. A dataset may be useful for direct machine processing.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Dataset", AlternateValue = "Dataset")]
        DataSet             = 2,

        /// <summary>
        /// The type represents a non-persistent, time-based occurrence.
        /// </summary>
        /// <remarks>
        ///     Metadata for an event provides descriptive information that is the basis for discovery of the purpose, location, duration, and responsible agents associated with an event. 
        ///     Examples include an exhibition, webcast, conference, workshop, open day, performance, battle, trial, wedding, tea party, conflagration.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Event", AlternateValue = "Event")]
        Event               = 4,

        /// <summary>
        /// The type represents a visual representation other than text.
        /// </summary>
        /// <remarks>
        ///     Examples include images and photographs of physical objects, paintings, prints, drawings, other images and graphics, animations and moving pictures, film, diagrams, maps, musical notation. 
        ///     Note that <see cref="Image"/> may include both electronic and physical representations.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Image", AlternateValue = "Image")]
        Image               = 8,

        /// <summary>
        /// The type represents a resource requiring interaction from the user to be understood, executed, or experienced.
        /// </summary>
        /// <remarks>
        ///     Examples include forms on Web pages, applets, multimedia learning objects, chat services, or virtual reality environments.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Interactive Resource", AlternateValue = "InteractiveResource")]
        InteractiveResource = 16,

        /// <summary>
        /// The type represents a series of visual representations imparting an impression of motion when shown in succession.
        /// </summary>
        /// <remarks>
        ///     Examples include animations, movies, television programs, videos, zoetropes, or visual output from a simulation. 
        ///     Instances of the type <see cref="MovingImage">Moving Image</see> must also be describable as instances of the broader type <see cref="Image"/>.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Moving Image", AlternateValue = "MovingImage")]
        MovingImage         = 32,

        /// <summary>
        /// The type represents an inanimate, three-dimensional object or substance.
        /// </summary>
        /// <remarks>
        ///     Note that digital representations of, or surrogates for, these objects should use <see cref="Image"/>, <see cref="Text"/> or one of the other types.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Physical Object", AlternateValue = "PhysicalObject")]
        PhysicalObject      = 64,

        /// <summary>
        /// The type represents a system that provides one or more functions.
        /// </summary>
        /// <remarks>
        ///     Examples include a photocopying service, a banking service, an authentication service, interlibrary loans, a Z39.50 or Web server.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Service", AlternateValue = "Service")]
        Service             = 128,

        /// <summary>
        /// The type represents a computer program in source or compiled form.
        /// </summary>
        /// <remarks>
        ///     Examples include a C source file, Microsoft Windows executable, or Perl script.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Software", AlternateValue = "Software")]
        Software            = 256,

        /// <summary>
        /// The type represents a resource primarily intended to be heard.
        /// </summary>
        /// <remarks>
        ///     Examples include a music playback file format, an audio compact disc, and recorded speech or sounds.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Sound", AlternateValue = "Sound")]
        Sound               = 512,

        /// <summary>
        /// The type represents a static visual representation.
        /// </summary>
        /// <remarks>
        ///     Examples include paintings, drawings, graphic designs, plans and maps. 
        ///     Recommended best practice is to assign the type <see cref="Text"/> to images of textual materials. 
        ///     Instances of the type <see cref="StillImage">Still Image</see> must also be describable as instances of the broader type <see cref="Image"/>.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Still Image", AlternateValue = "StillImage")]
        StillImage          = 1024,

        /// <summary>
        /// The type represents a resource consisting primarily of words for reading.
        /// </summary>
        /// <remarks>
        ///     Examples include books, letters, dissertations, poems, newspapers, articles, archives of mailing lists. 
        ///     Note that facsimiles or images of texts are still of the genre <see cref="Text"/>.
        /// </remarks>
        [EnumerationMetadata(DisplayName = "Text", AlternateValue = "Text")]
        Text                = 2048
    }
}