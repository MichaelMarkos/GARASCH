using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NewGaras.Infrastructure.Entities;

public partial class CompetitionDayResource
{
    [Key]
    public int Id { get; set; }

    public int CompetitionDayId { get; set; }

    [Column("bookId")]
    [StringLength(250)]
    public string BookId { get; set; }

    [Column("fromChapterId")]
    [StringLength(250)]
    public string FromChapterId { get; set; }

    [Column("toChapterId")]
    [StringLength(250)]
    public string ToChapterId { get; set; }

    [Column("youtubeLink")]
    public string YoutubeLink { get; set; }

    [Column("youtubeTitle")]
    [StringLength(450)]
    public string YoutubeTitle { get; set; }

    [Column("youtubeLink2")]
    public string YoutubeLink2 { get; set; }

    [Column("youtubeTitle2")]
    [StringLength(450)]
    public string YoutubeTitle2 { get; set; }

    [Column("youtubeLink3")]
    public string YoutubeLink3 { get; set; }

    [Column("youtubeTitle3")]
    [StringLength(450)]
    public string YoutubeTitle3 { get; set; }

    [Column("PDFLink1")]
    public string Pdflink1 { get; set; }

    [Column("PDTitle1")]
    [StringLength(450)]
    public string Pdtitle1 { get; set; }

    [Column("PDFLink2")]
    public string Pdflink2 { get; set; }

    [Column("PDTitle2")]
    [StringLength(450)]
    public string Pdtitle2 { get; set; }

    [Column("PDFLink3")]
    public string Pdflink3 { get; set; }

    [Column("PDTitle3")]
    [StringLength(450)]
    public string Pdtitle3 { get; set; }

    public DateTime? CreationDate { get; set; }

    [StringLength(250)]
    public string CreationBy { get; set; }

    [Column("bookName")]
    [StringLength(250)]
    public string BookName { get; set; }

    [Column("fromChapterName")]
    [StringLength(250)]
    public string FromChapterName { get; set; }

    [Column("toChapterName")]
    [StringLength(250)]
    public string ToChapterName { get; set; }

    [ForeignKey("CompetitionDayId")]
    [InverseProperty("CompetitionDayResources")]
    public virtual CompetitionDay CompetitionDay { get; set; }
}
