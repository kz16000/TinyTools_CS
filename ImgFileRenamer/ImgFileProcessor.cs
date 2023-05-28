using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImgFileRenamer
{
    //****************************************************************
    //  class ImgFileProcessor :
    //
    //  - Analyzes given file path.
    //  - If the filname satisfies the following format:
    //     "IMG_####.jpg" (#=number)
    //  - then rename to:
    //     "IMG_YYYYMMDD_####.jpg"
    //     - where (YYYYMMDD) part contains the date information
    //       extacted from the EXIF header of jpeg file.
    //     - (####) part preserves the number included in the
    //       original file name.
    //
    //  - Ignores other filenames.
    //  - Doesn't rename if it failes extraction from the EXIF header.
    //****************************************************************
    class ImgFileProcessor
    {
        const string s_RegExPattern0 = @"(\S+)\\([\w_]+\.jpg)";
        const string s_RegExPattern1 = @"IMG_([\d]+)\.jpg";

        //****************************************************************
        //  Constructor
        //****************************************************************
        public ImgFileProcessor( string filePath )
        {
            m_FullPath = filePath;
        }

        //****************************************************************
        //  Main part of the processing
        //****************************************************************
        public void Process()
        {
            if (!ExtractFilePath())
            {
                Console.WriteLine("SKIP: " + m_FullPath);
                return;
            }

            if (!ReadExifDate())
            {
                Console.WriteLine(m_OrgFileName + " -> Failed to read EXIF date information.");
                return;
            }

            if (m_IsRenameNeeded)
            {
                Console.WriteLine(m_OrgFileName + " -> " + m_NewFileName);
                if (!RenameFile())
                {
                    Console.WriteLine(" -> Failed rename operation.");
                }
            }
            else
            {
                Console.WriteLine(m_OrgFileName + " (No rename / " + m_NewFileName + ")");
            }
        }

        //****************************************************************
        //  Extracts/analyzes the file path
        //****************************************************************
        private bool ExtractFilePath()
        {
            Regex re0 = new Regex(s_RegExPattern0, RegexOptions.IgnoreCase);
            Match m0 = re0.Match(m_FullPath);

            if (!m0.Success)
            {
                // The file is not jpg.
                return false;
            }

            // Separates folder path and file name part.
            m_FolderPath = m0.Groups[1].ToString();
            m_OrgFileName = m0.Groups[2].ToString();

            // Checks if the file name matches "IMG_####.jpg" format.
            Regex re1 = new Regex(s_RegExPattern1, RegexOptions.IgnoreCase);
            Match m1 = re1.Match(m_OrgFileName);

            if (m1.Success)
            {
                m_ImageId = m1.Groups[1].ToString();
                m_IsRenameNeeded = true;
            }
            else
            {
                m_ImageId = "";
                m_IsRenameNeeded = false;
            }

            return true;
        }
        
        //****************************************************************
        //  Reads an EXIF Date tag
        //****************************************************************
        private bool ReadExifDate()
        {
            Bitmap jpgImg = new Bitmap(m_FullPath);

            PropertyItem datePropItem;
            try
            {
                datePropItem = jpgImg.GetPropertyItem(0x9003);
            }
            catch ( ArgumentException )
            {
                jpgImg.Dispose();
                return false;
            }

            string dateStr = Encoding.ASCII.GetString(datePropItem.Value, 0, 19);
            if (m_IsRenameNeeded)
            {
                dateStr = dateStr.Remove(10).Remove(7, 1).Remove(4, 1);
                m_NewFileName = "IMG_" + dateStr + "_" + m_ImageId + ".JPG";
            }
            else
            {
                m_NewFileName = dateStr;
            }

            jpgImg.Dispose();
            return true;
        }

        //****************************************************************
        //  Renames the file
        //****************************************************************
        private bool RenameFile()
        {
            string newFullPath = m_FolderPath + "\\" + m_NewFileName;

            try
            {
                File.Move(m_FullPath, newFullPath);
            }
            catch // ( Exception e )
            {
                // Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        //****************************************************************
        //  Private member variables
        //****************************************************************
        private string m_FullPath;
        private string m_FolderPath;
        private string m_ImageId;
        private string m_OrgFileName;
        private string m_NewFileName;
        private bool m_IsRenameNeeded;
    
    }
}
