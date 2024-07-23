using Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Web_App.Models
{
    public class RemarkModel
    {
        [Required]
        [DisplayName("Text*")]
        [StringLength(255)]
        public string Text { get; set; }

        [Required]
        [DisplayName("Visible For Patient?*")]
        public bool IsVisibleForClient { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PostedOn { get; set; }

        public int RemarkMadeById { get; set; }

        public IEnumerable<Remark> Remarks { get; set; }

        public Remark Forge()
        {
            Remark remark = new Remark();
            remark.Text = Text;
            remark.IsVisibleForClient = IsVisibleForClient;
            remark.PostedOn = PostedOn;
            remark.RemarkMadeById = RemarkMadeById;
            return remark;
        }

        public static RemarkModel Create(Remark remark)
        {
            RemarkModel remarkModel = new RemarkModel();
            remarkModel.Text = remark.Text;
            remarkModel.IsVisibleForClient = remark.IsVisibleForClient;
            remarkModel.PostedOn = remark.PostedOn;
            remarkModel.RemarkMadeById = remark.RemarkMadeById.HasValue ? remark.RemarkMadeById.Value : -1;
            return remarkModel;
        }
    }
}