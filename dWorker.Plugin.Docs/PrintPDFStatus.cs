using Syncfusion.Blazor.PdfViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dWorker.Plugin.Docs
{
    /// <summary>
    /// Describe the current status of a job of printing
    /// </summary>
    public class PrintPDFStatus
    {
        /// <summary>
        /// Array with the number of the pages that has to be printed (normally all)
        /// </summary>
        private readonly int[] _pageNumbers;

        /// <summary>
        /// Indicates the next page that has to be printed
        /// </summary>
        private int _cursor;

        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="document">Document that need to be printed</param>
        public PrintPDFStatus(PdfRenderer document)
        {

            int[] pageNumbers = new int[document.PageCount];
            for (int i = 0; i < document.PageCount; i++)
                pageNumbers[i] = i;
            _pageNumbers = pageNumbers;
            Document = document;
            _cursor = 0;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="document">Document that need to be printed</param>
        /// <param name="pageNumbers">Pages that needs to be printed</param>
        public PrintPDFStatus(PdfRenderer document, int[] pageNumbers)
        {
            _pageNumbers = pageNumbers;
            Document = document;
            _cursor = 0;
        }

        /// <summary>
        /// Current document
        /// </summary>
        public PdfRenderer Document { get; }

        /// <summary>
        /// Current page that needs to be printed
        /// </summary>
        public int CurrentPageIndex => _pageNumbers[_cursor];

        /// <summary>
        /// Move the cursot to the next page (return true if it is possible)
        /// </summary>
        /// <returns>return true if there is another page to print, otherwise false.</returns>
        public bool AdvanceToNextPage()
        {
            if (_cursor >= _pageNumbers.Length)
                return false;

            _cursor++;
            return _cursor < _pageNumbers.Length;
        }
    }
}
