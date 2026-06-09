using CncMes.Application.Reporting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CncMes.Infrastructure.Reporting;

/// <summary>
/// Sipariş bazlı kar/zarar raporunu QuestPDF ile PDF'e döker.
/// QuestPDF Community lisansı (ücretsiz, küçük ölçek) statik olarak set edilir.
/// </summary>
public class ProfitLossReportPdf : IProfitLossReportPdf
{
    static ProfitLossReportPdf()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] BuildOrderReport(OrderProfitLossDto s, IReadOnlyList<OrderCostLineDto> lines)
    {
        var profitColor = s.Profit >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Black));

                // --- Başlık ---
                page.Header().Column(col =>
                {
                    col.Item().Text("CncMes — Kar/Zarar Raporu")
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Text($"Sipariş #{s.OrderId} — {s.CustomerName}")
                        .FontSize(12).SemiBold();
                    col.Item().Text($"Sipariş Tarihi: {s.OrderDate:dd.MM.yyyy}   ·   Durum: {s.Status}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(12);

                    if (!s.IsFullyCosted)
                    {
                        col.Item().Background(Colors.Orange.Lighten4).Padding(6).Text(
                            $"⚠ Bu siparişin {s.TotalJobs} işinden {s.CompletedJobs} tanesi tamamlandı. " +
                            "Maliyet henüz kısmi — kar/zarar geçicidir.")
                            .FontSize(9).FontColor(Colors.Orange.Darken3);
                    }

                    // --- Özet kutusu ---
                    col.Item().Text("Özet").FontSize(13).Bold();
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c => { c.RelativeColumn(2); c.RelativeColumn(1); });

                        SummaryRow(t, "Gelir (sipariş tutarı)", s.Revenue, false);
                        SummaryRow(t, "Hammadde", s.MaterialCost, true);
                        SummaryRow(t, "İşçilik", s.LaborCost, true);
                        SummaryRow(t, "Elektrik", s.ElectricityCost, true);
                        SummaryRow(t, "Takım Aşınması", s.ToolWearCost, true);
                        SummaryRow(t, "Toplam Maliyet", s.TotalCost, true, bold: true);
                    });

                    // --- Kar / Marj ---
                    col.Item().PaddingTop(4).Row(r =>
                    {
                        r.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Kar / Zarar").FontSize(11).SemiBold();
                            c.Item().Text($"{s.Profit:N2} TL").FontSize(20).Bold().FontColor(profitColor);
                        });
                        r.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().AlignRight().Text("Kar Marjı").FontSize(11).SemiBold();
                            c.Item().AlignRight().Text($"%{s.MarginPercent:N1}")
                                .FontSize(20).Bold().FontColor(profitColor);
                        });
                    });

                    // --- Maliyet kalemleri detayı ---
                    if (lines.Count > 0)
                    {
                        col.Item().PaddingTop(6).Text("Maliyet Kalemleri").FontSize(13).Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn(3); // parça
                                c.RelativeColumn(2); // tür
                                c.RelativeColumn(4); // detay
                                c.RelativeColumn(2); // tutar
                            });

                            t.Header(h =>
                            {
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                                    .Text("Parça").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                                    .Text("Kalem").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                                    .Text("Hesap").SemiBold().FontSize(9);
                                h.Cell().Background(Colors.Grey.Lighten3).Padding(4)
                                    .AlignRight().Text("Tutar (TL)").SemiBold().FontSize(9);
                            });

                            foreach (var l in lines)
                            {
                                BodyCell(t, l.PartName);
                                BodyCell(t, l.TypeLabel);
                                BodyCell(t, l.Detail ?? "—", small: true);
                                BodyCell(t, l.AmountTL.ToString("N2"), right: true);
                            }
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span($"CncMes · {DateTime.Now:dd.MM.yyyy HH:mm} · ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    x.Span("sayfa ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                    x.Span(" / ").FontSize(8).FontColor(Colors.Grey.Darken1);
                    x.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        return document.GeneratePdf();
    }

    private static void SummaryRow(TableDescriptor t, string label, decimal amount, bool isCost, bool bold = false)
    {
        var labelText = t.Cell().PaddingVertical(2).Text(label);
        if (bold) labelText.SemiBold();

        var sign = isCost ? "−" : "";
        var valText = t.Cell().PaddingVertical(2).AlignRight().Text($"{sign}{amount:N2} TL");
        if (bold) valText.SemiBold();
    }

    private static void BodyCell(TableDescriptor t, string text, bool right = false, bool small = false)
    {
        var c = t.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4);
        var span = (right ? c.AlignRight() : c).Text(text);
        if (small) span.FontSize(8).FontColor(Colors.Grey.Darken1);
    }
}
