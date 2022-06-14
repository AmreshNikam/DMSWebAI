
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMSWebAI.Database
{
    public class Documents
    {
        public int DocumentID { get; set; }
        public string CompCode { get; set; }
        public int DocumentType { get; set; }
        public string DocumentLabel { get; set; }
        public string DocumentFileName { get; set; }
        public byte[] ActualFile { get; set; }
        public bool BPCodeList { get; set; }
        public int? LongTextID { get; set; }
        public bool Status { get; set; }
        public DateTime UploadDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string UploadBy { get; set; }
        //*******************From Other tables******************
        public List<string> BPCodeListArray { get; set; }
        public string Text { get; set; }
        //*****************Derived attrivute******************
        public string DocumentDesc { get; set; }
    }
    public class DocumentsType
    {
        public int ID { get; set; }
        public string CompCode { get; set; }
        public string DocumentDesc { get; set; }
    }
    }
