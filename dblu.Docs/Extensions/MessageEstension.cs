using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MimeKit;
using System.IO;
using MimeKit.Tnef;

namespace dblu.Docs.Extensions
{
    public static class MimeMessageExtensions
    {
        public static IEnumerable<MimeEntity> Allegati(this MimeMessage message)
        { 
            Dictionary<string,MimeEntity> elenco = new Dictionary<string, MimeEntity>();
            List<string> inclusi = new List<string>();
            List<string> esclusi= new List<string>();
            try
            {
                int i = 0;
                foreach (MimeEntity all in message.Attachments)
                {
                    
                    if (all.IsAttachment)
                    {
                        //all.WriteTo("d:\\temp\\winmail.dat");

                        if (all.GetType() == typeof(MimeKit.Tnef.TnefPart))
                        {
                            esclusi.Add(all.IdAllegato());
                            
                            var t = (MimeKit.Tnef.TnefPart)all;
                            foreach (var all1 in t.ExtractAttachments())
                            {
                                if (all1.IsAttachment || all1.ContentType.MediaType=="image") { 
                                    i++;
                                    inclusi.Add(all1.IdAllegato());
                                    elenco.Add(all1.NomeAllegato(i), all1);
                                }
                            }
                        }
                        else {
                            i++;
                            inclusi.Add(all.IdAllegato());
                            elenco.Add(all.NomeAllegato(i), all);
                        } 
                    }
                    else
                    {
                        //esclusi.Add(all.NomeAllegato(i));
                        esclusi.Add(all.IdAllegato());
                        //elenco.Add(all.NomeAllegato(), all);
                    }
                }
            //&& x.IsAttachment == false
                IEnumerable<MimeEntity> att = message.BodyParts.Where(x => x.ContentType.Name != null  ||  x.ContentType.MediaType=="image" ).ToList();
                foreach (MimeEntity all in att)
                {
                    //var nome = all.NomeAllegato(i);
                    var nome = all.IdAllegato();
                    if ( all.GetType() != typeof(MimeKit.Tnef.TnefPart) 
                        && !inclusi.Contains(nome) 
                        && !esclusi.Contains(nome) )
                    {
                        i++;
                        elenco.Add(all.NomeAllegato(i), all);
                    }
                }
            }
            catch (Exception ex) {
                Debug.Assert(false);
            }
            return elenco.Values.ToList();
        }

        public static string ToHtml(this MimeMessage Messaggio)
        {
            var htxt = Messaggio.HtmlBody == null ? "" : Messaggio.HtmlBody;
            if (htxt == "")
            {
                // cerco allegati di tipo testo/html
                var htmlContent = Messaggio.BodyParts.OfType<TextPart>().FirstOrDefault(x => x.IsAttachment && x.IsHtml && x.FileName is null);
                if (htmlContent != null)
                {
                    htxt = htmlContent.Text;
                }
            }

            if (htxt == "")
            {
                // controlla presenza testo rtf exchange/outlook (winmail.dat)
                // estrae rtf e converte in pdf
                var rtxt = "";
                try
                {
                    var tnef = Messaggio.BodyParts.OfType<TnefPart>().FirstOrDefault();
                    if (tnef != null)
                    {
                        foreach (var attachment in tnef.ExtractAttachments())
                        {
                            var mime_part = attachment as MimePart;
                            var text = attachment as TextPart;
                            if (text != null)
                            {
                                if (text.IsHtml)
                                {
                                    htxt = text.Text;
                                }
                                else
                                {
                                    rtxt += text.Text;
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                if (rtxt != "")
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    htxt = RtfPipe.Rtf.ToHtml(rtxt);
                }
            }

            return htxt;
        }


        public static string NomeAllegato(this MimeEntity attachment, int Index=-1)
        {
            string fileName = $"non definito_{Index}";
            if (attachment is MessagePart)
            {
                fileName = attachment.ContentDisposition?.FileName;
                if (string.IsNullOrEmpty(fileName))
                    fileName = "email-allegata.eml";
            }
            else
            {
                var part = (MimePart)attachment;
                if(part.FileName!=null || part.ContentType.MediaType=="image")
                {
                    var nome = ""; var ext = "";
                    List<string> myExt = new List<string>{ ".pdf", ".jpg", ".jpeg", ".png" };  
                    List<string> myTypes = new List<string> { "pdf", "jpeg" };
                    if (part.FileName == null)
                    {
                        if (!string.IsNullOrEmpty(part.ContentType.MediaSubtype) && myTypes.Contains(part.ContentType.MediaSubtype))
                        {
                            nome = part.ContentType.MediaType;
                            ext = "." + part.ContentType.MediaSubtype;
                        }
                    }
                    else {
                        nome =  Path.GetFileNameWithoutExtension(part.FileName);
                        ext =  Path.GetExtension(part.FileName).ToLower();
                        if (!myExt.Contains(ext) && !string.IsNullOrEmpty(part.ContentType.MediaSubtype)){
                           if (myTypes.Contains(part.ContentType.MediaSubtype))
                            { 
                                nome = part.FileName;
                                ext = "." + part.ContentType.MediaSubtype;                    
                            }
                        }                    
                    }
                    if (Index>=0)
                        fileName = $"{nome}_{Index}{ext}";
                    else
                        fileName = $"{nome}{ext}";
                }
            }
            return fileName;
            }


        public static string IdAllegato(this MimeEntity attachment)
        {
            if (attachment.ContentId == null)
            {
                return attachment.NomeAllegato();
            }
            return attachment.ContentId;
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
