using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MimeKit;
using System.IO;

namespace dblu.Docs.Extensions
{
    public static class MimeMessageExtensions
    {
        public static IEnumerable<MimeEntity> Allegati(this MimeMessage message)
        { 
            Dictionary<string,MimeEntity> elenco = new Dictionary<string, MimeEntity>();
            List<string> esclusi= new List<string>();
            try
            {
                foreach (MimeEntity all in message.Attachments)
                {
                    if (all.IsAttachment)
                    {
                        if (all.GetType() == typeof(MimeKit.Tnef.TnefPart))
                        {
                            esclusi.Add(all.NomeAllegato());
                            
                            var t = (MimeKit.Tnef.TnefPart)all;
                            foreach (var all1 in t.ExtractAttachments())
                            {
                                if (all1.IsAttachment) { 
                                    elenco.Add(all1.NomeAllegato(), all1);
                                }
                            }
                        }
                        else {
                            
                            elenco.Add(all.NomeAllegato(), all);
                        } 
                    }
                    else
                    {
                        esclusi.Add(all.NomeAllegato());
                        //elenco.Add(all.NomeAllegato(), all);
                    }
                }
            //&& x.IsAttachment == false
                IEnumerable<MimeEntity> att = message.BodyParts.Where(x => x.ContentType.Name != null).ToList();
                foreach (MimeEntity all in att)
                {
                    var nome = all.NomeAllegato();
                    if ( all.GetType() != typeof(MimeKit.Tnef.TnefPart) 
                        && !elenco.ContainsKey(nome) 
                        && !esclusi.Contains(nome) )
                    {
                        elenco.Add(nome, all);
                    }
                }
            }
            catch (Exception ex) {
                Debug.Assert(false);
            }
            return elenco.Values.ToList();
        }


        public static string NomeAllegato(this MimeEntity attachment)
        {
            string fileName = "non definito";
            if (attachment is MessagePart)
            {
                fileName = attachment.ContentDisposition?.FileName;
                if (string.IsNullOrEmpty(fileName))
                    fileName = "email-allegata.eml";
            }
            else
            {
                var part = (MimePart)attachment;
                if(part.FileName!=null)
                fileName = part.FileName;
            }
            return fileName;
            }

    }

    public static class PdfStreamExtensions
    {
        private const byte LineFeed = (byte)'\n';
        private const byte CarriageReturn = (byte)'\r';
        private static readonly Encoding PdfEncoding = Encoding.UTF8;

        public static void Write(this Stream stream, string text)
        {
            byte[] bytes = PdfEncoding.GetBytes(text);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static long IndexOf(this Stream stream, string text)
        {
            byte[] bytes = PdfEncoding.GetBytes(text);

            return stream.IndexOf(bytes);
        }

        public static long IndexOf(this Stream stream, byte[] byteSequence)
        {
            stream.Seek(0, SeekOrigin.Begin);

            int byteSequenceIndexToCheck = 0;

            while (!IsEndOfStream(stream))
            {
                byte b = (byte)stream.ReadByte();

                if (b.Equals(byteSequence[byteSequenceIndexToCheck]))
                {
                    byteSequenceIndexToCheck++;
                }
                else if (byteSequenceIndexToCheck != 0)
                {
                    stream.Seek(-byteSequenceIndexToCheck, SeekOrigin.Current);
                    byteSequenceIndexToCheck = 0;
                }

                if (byteSequenceIndexToCheck == byteSequence.Length)
                {
                    long oldPosition = stream.Position;
                    stream.Seek(-(byteSequence.Length + 1), SeekOrigin.Current);
                    byte previousByte = (byte)stream.ReadByte();
                    stream.Seek(oldPosition, SeekOrigin.Begin);

                    if (previousByte == 10 || previousByte == 13 || previousByte == 20)
                    {
                        break;
                    }
                    else
                    {
                        byteSequenceIndexToCheck = 0;
                    }
                }
            }

            if (byteSequenceIndexToCheck == byteSequence.Length)
            {
                long startPosition = stream.Position - byteSequenceIndexToCheck;

                return startPosition;
            }
            else
            {
                return -1;
            }
        }

        public static string ReadLine(this Stream stream)
        {
            StringBuilder stringBuilder = new StringBuilder();
            byte b;
            while (!IsEndOfStream(stream) && !IsLineFeed(b = (byte)stream.ReadByte()))
            {
                if (b != CarriageReturn)
                {
                    stringBuilder.Append((char)b);
                }
                else if (!IsEndOfStream(stream) && !IsLineFeed(b = stream.Peek()))
                {
                    break;
                }
            }

            return stringBuilder.ToString();
        }

        public static byte Peek(this Stream stream)
        {
            byte b = (byte)stream.ReadByte();
            stream.Seek(-1, SeekOrigin.Current);

            return b;
        }

        private static bool IsLineFeed(byte b)
        {
            return b == LineFeed;
        }

        private static bool IsEndOfStream(Stream stream)
        {
            return stream.Position >= stream.Length;
        }

        public static MemoryStream RepairPdfWithSimpleCrossReferenceTable(this Stream corruptedPdfFile)
        {
            string xrefKeyword = "xref";
            long xrefIndex = corruptedPdfFile.IndexOf(xrefKeyword);
            corruptedPdfFile.Seek(0, SeekOrigin.Begin);
            MemoryStream repairedDocument = new MemoryStream();
            corruptedPdfFile.CopyTo(repairedDocument);

            corruptedPdfFile.Seek(xrefIndex, SeekOrigin.Begin);
            corruptedPdfFile.ReadLine();
            string line = corruptedPdfFile.ReadLine();
            string[] tokens = line.Split(' ');
            string secondToken = tokens[1].Trim();
            int numberOfObjects = int.Parse(secondToken);
            corruptedPdfFile.ReadLine();
            long nextLinePosition = corruptedPdfFile.Position;

            for (int pdfId = 1; pdfId < numberOfObjects; pdfId++)
            {
                repairedDocument.Seek(nextLinePosition, SeekOrigin.Begin);
                long pdfOffset = corruptedPdfFile.IndexOf(string.Format("{0} 0 obj", pdfId));
                repairedDocument.Write(string.Format("{0} 00000 n", pdfOffset.ToString().PadLeft(10, '0')));

                corruptedPdfFile.Seek(nextLinePosition, SeekOrigin.Begin);
                corruptedPdfFile.ReadLine();
                nextLinePosition = corruptedPdfFile.Position;
            }

            long startXrefPosition = corruptedPdfFile.IndexOf("startxref");
            corruptedPdfFile.Seek(startXrefPosition, SeekOrigin.Begin);
            corruptedPdfFile.ReadLine();
            long xrefOffsetPosition = corruptedPdfFile.Position;
            string corruptedOffset = corruptedPdfFile.ReadLine();
            repairedDocument.Seek(xrefOffsetPosition, SeekOrigin.Begin);
            repairedDocument.Write(xrefIndex.ToString().PadRight(corruptedOffset.Length));

            return repairedDocument;
        }




    }




}
