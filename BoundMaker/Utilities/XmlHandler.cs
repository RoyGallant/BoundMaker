using BoundMaker.Models;
using BoundMaker.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;

namespace BoundMaker.Services
{
    public static class XmlHandler
    {

        public static BoundMakerFile ReadXmlDocument(string fileLocation)
        {
            if (fileLocation == null) throw new ArgumentNullException(nameof(fileLocation));

            var file = new BoundMakerFile();
            var settings = new XmlReaderSettings { IgnoreComments = true, IgnoreWhitespace = true };

            using (var reader = XmlReader.Create(fileLocation, settings))
            {
                reader.Read();
                if (!reader.Name.Equals("xml", StringComparison.OrdinalIgnoreCase)) throw new XmlException();

                reader.Read();
                if (!reader.Name.Equals("Bound")) throw new XmlException();

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        if (reader.Name == "Locations")
                        {
                            file.Locations = ReadLocations(reader).ToList();
                        }
                        else if (reader.Name == "Tiles")
                        {
                            file.Tiles = ReadTiles(reader).ToList();
                        }
                        else if (reader.Name == "Sequences")
                        {
                            // Will error if XML document reads sequences before locations, due to (bad) dependency
                            file.Sequences = ReadSequences(reader, file.Locations).ToList();
                        }
                    }
                }
            }
            return file;
        }

        public static void WriteXmlDocument(string fileLocation, BoundMakerFile file)
        {
            if (fileLocation == null) throw new ArgumentNullException(nameof(fileLocation));
            if (file == null) throw new ArgumentNullException(nameof(file));

            var settings = new XmlWriterSettings { Encoding = Encoding.UTF8, Indent = true };
            using (var writer = XmlWriter.Create(fileLocation, settings))
            {
                writer.WriteStartElement("Bound");
                WriteLocations(writer, file.Locations);
                WriteTiles(writer, file.Tiles);
                WriteSequences(writer, file.Sequences);
                writer.WriteEndElement();
            }
        }


        private static IEnumerable<MapLocation> ReadLocations(XmlReader reader)
        {
            using (XmlReader locationReader = reader.ReadSubtree())
            {
                while (locationReader.Read())
                {
                    if (locationReader.Name == "Location")
                    {
                        int x = int.Parse(locationReader.GetAttribute("x"));
                        int y = int.Parse(locationReader.GetAttribute("y"));
                        int w = int.Parse(locationReader.GetAttribute("w"));
                        int h = int.Parse(locationReader.GetAttribute("h"));
                        var location = new MapLocation(x, y, w, h) { LocationName = locationReader.GetAttribute("LocationName") };
                        yield return location;
                    }
                }
            }
        }

        private static IEnumerable<MapTerrainTile> ReadTiles(XmlReader reader)
        {
            using (XmlReader tileReader = reader.ReadSubtree())
            {
                while (tileReader.Read())
                {
                    if (tileReader.Name == "Tile")
                    {
                        var tile = new MapTerrainTile();
                        tile.SetTerrain(tileReader.GetAttribute("Terrain"));
                        Grid.SetRow(tile, int.Parse(tileReader.GetAttribute("Row")));
                        Grid.SetColumn(tile, int.Parse(tileReader.GetAttribute("Column")));
                        yield return tile;
                    }
                }
            }
        }

        private static IEnumerable<BoundSequence> ReadSequences(XmlReader reader, IEnumerable<MapLocation> locations)
        {
            var locationDictionary = locations.ToDictionary(x => x.LocationName);

            using (XmlReader sequenceReader = reader.ReadSubtree())
            {
                while (sequenceReader.Read())
                {
                    if (sequenceReader.Name == "Sequence")
                    {
                        var sequence = new BoundSequence
                        {
                            WaitTime = int.Parse(sequenceReader.GetAttribute("WaitTime"))
                        };
                        XmlReader sequenceLocationReader = sequenceReader.ReadSubtree();
                        while (sequenceLocationReader.Read())
                        {
                            if (sequenceLocationReader.Name == "Location")
                            {
                                sequence.SetLocationState(locationDictionary[sequenceLocationReader.GetAttribute("LocationName")], sequenceLocationReader.GetAttribute("State"));
                            }
                        }
                        yield return sequence;
                    }
                }
            }
        }

        private static void WriteLocations(XmlWriter writer, IEnumerable<MapLocation> locations)
        {
            writer.WriteStartElement("Locations");
            if (locations != null)
            {
                foreach (MapLocation location in locations)
                {
                    writer.WriteStartElement("Location");
                    writer.WriteAttributeString("x", Canvas.GetLeft(location).ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("y", Canvas.GetTop(location).ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("w", location.Width.ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("h", location.Height.ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("LocationName", location.LocationName);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        private static void WriteTiles(XmlWriter writer, IEnumerable<MapTerrainTile> tiles)
        {
            writer.WriteStartElement("Tiles");
            if (tiles != null)
            {
                foreach (MapTerrainTile tile in tiles.Where(t => t.Terrain!= "null"))
                {
                    writer.WriteStartElement("Tile");
                    writer.WriteAttributeString("Row", Grid.GetRow(tile).ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("Column", Grid.GetColumn(tile).ToString(CultureInfo.InvariantCulture));
                    writer.WriteAttributeString("Terrain", tile.Terrain);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        private static void WriteSequences(XmlWriter writer, IEnumerable<BoundSequence> sequences)
        {
            writer.WriteStartElement("Sequences");
            if (sequences != null)
            {
                foreach (BoundSequence sequence in sequences)
                {
                    writer.WriteStartElement("Sequence");
                    writer.WriteAttributeString("WaitTime", sequence.WaitTime.ToString(CultureInfo.InvariantCulture));
                    foreach (KeyValuePair<MapLocation, string> state in sequence.States.Where(x => x.Value != "default"))
                    {
                        writer.WriteStartElement("Location");
                        writer.WriteAttributeString("LocationName", state.Key.LocationName);
                        writer.WriteAttributeString("State", state.Value);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }
    }
}
