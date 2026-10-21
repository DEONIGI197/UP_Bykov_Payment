using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Forms.DataVisualization.Charting;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;

namespace UP_Bykov_Payment.Pages
{
    /// <summary>
    /// Логика взаимодействия для DiagrammPage.xaml
    /// </summary>
    public partial class DiagrammPage : Page
    {
        private DB_Bykov_PaymentEntities _context = new DB_Bykov_PaymentEntities();

        public DiagrammPage()
        {
            InitializeComponent();

            // область построения диаграммы
            ChartPayments.ChartAreas.Add(new ChartArea("Main"));

            // набор данных
            var currentSeries = new Series("Платежи")
            {
                IsValueShownAsLabel = true
            };
            ChartPayments.Series.Add(currentSeries);

            // загружаем данные в списки
            CmbUser.ItemsSource = _context.User.ToList();
            CmbDiagram.ItemsSource = Enum.GetValues(typeof(SeriesChartType));

            // чтобы диаграмма сразу была видна
            CmbDiagram.SelectedItem = SeriesChartType.Column;
            CmbUser.SelectedIndex = 0;
        }

        // перестраиваем диаграмму при смене пользователя или типа
        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser &&
                CmbDiagram.SelectedItem is SeriesChartType currentType)
            {
                Series currentSeries = ChartPayments.Series.FirstOrDefault();
                currentSeries.ChartType = currentType;
                currentSeries.Points.Clear();

                var categoriesList = _context.Category.ToList();
                foreach (var category in categoriesList)
                {
                    currentSeries.Points.AddXY(category.Name,
                        _context.Payment.ToList()
                            .Where(u => u.UserID == currentUser.ID && u.CategoryID == category.ID)
                            .Sum(u => u.Price * u.Num));
                }
            }
        }

        // папка для отчетов рядом с программой (создается, если ее нет)
        private string GetReportsFolder()
        {
            string folder = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Отчеты");
            System.IO.Directory.CreateDirectory(folder);
            return folder;
        }

        // экспорт в Excel
        private void BtnExcel_Click(object sender, RoutedEventArgs e)
        {
            Excel.Application application = null;
            try
            {
                // список пользователей, отсортированный по ФИО
                var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();
                if (allUsers.Count == 0)
                {
                    MessageBox.Show("Нет пользователей для выгрузки!");
                    return;
                }

                // новая книга, листов столько же, сколько пользователей
                application = new Excel.Application();
                application.SheetsInNewWorkbook = allUsers.Count();
                Excel.Workbook workbook = application.Workbooks.Add(Type.Missing);

                decimal grandTotal = 0;
                List<string> usedNames = new List<string>();

                for (int i = 0; i < allUsers.Count(); i++)
                {
                    int startRowIndex = 1;
                    Excel.Worksheet worksheet = application.Worksheets.Item[i + 1];
                    // имя листа в Excel не длиннее 31 символа
                    string sheetName = allUsers[i].FIO;
                    if (sheetName.Length > 27)
                        sheetName = sheetName.Substring(0, 27);
                    // два листа с одинаковым именем нельзя - добавляем номер
                    if (usedNames.Contains(sheetName))
                        sheetName = sheetName + " (" + (i + 1) + ")";
                    usedNames.Add(sheetName);
                    worksheet.Name = sheetName;

                    // названия колонок
                    worksheet.Cells[1][startRowIndex] = "Дата платежа";
                    worksheet.Cells[2][startRowIndex] = "Название";
                    worksheet.Cells[3][startRowIndex] = "Стоимость";
                    worksheet.Cells[4][startRowIndex] = "Количество";
                    worksheet.Cells[5][startRowIndex] = "Сумма";
                    Excel.Range columlHeaderRange = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[5][1]];
                    columlHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                    columlHeaderRange.Font.Bold = true;
                    startRowIndex++;

                    // платежи пользователя, сгруппированные по категориям
                    var userCategories = allUsers[i].Payment.OrderBy(u => u.Date).GroupBy(u => u.Category).OrderBy(u => u.Key.Name);

                    foreach (var groupCategory in userCategories)
                    {
                        // название категории
                        Excel.Range headerRange = worksheet.Range[worksheet.Cells[1][startRowIndex], worksheet.Cells[5][startRowIndex]];
                        headerRange.Merge();
                        headerRange.Value = groupCategory.Key.Name;
                        headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        headerRange.Font.Italic = true;
                        startRowIndex++;

                        // платежи
                        foreach (var payment in groupCategory)
                        {
                            worksheet.Cells[1][startRowIndex] = payment.Date.ToString("dd.MM.yyyy");
                            worksheet.Cells[2][startRowIndex] = payment.Name;
                            worksheet.Cells[3][startRowIndex] = payment.Price;
                            (worksheet.Cells[3][startRowIndex] as Excel.Range).NumberFormat = "0.00";
                            worksheet.Cells[4][startRowIndex] = payment.Num;
                            worksheet.Cells[5][startRowIndex].Formula = $"=C{startRowIndex}*D{startRowIndex}";
                            (worksheet.Cells[5][startRowIndex] as Excel.Range).NumberFormat = "0.00";
                            grandTotal += payment.Price * payment.Num;
                            startRowIndex++;
                        }

                        // итого по категории
                        Excel.Range sumRange = worksheet.Range[worksheet.Cells[1][startRowIndex], worksheet.Cells[4][startRowIndex]];
                        sumRange.Merge();
                        sumRange.Value = "ИТОГО:";
                        sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;
                        worksheet.Cells[5][startRowIndex].Formula = $"=SUM(E{startRowIndex - groupCategory.Count()}:" +
                            $"E{startRowIndex - 1})";
                        (worksheet.Cells[5][startRowIndex] as Excel.Range).NumberFormat = "0.00";
                        sumRange.Font.Bold = worksheet.Cells[5][startRowIndex].Font.Bold = true;
                        startRowIndex++;
                    }

                    // границы таблицы
                    Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1][1], worksheet.Cells[5][startRowIndex - 1]];
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle =
                    rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;

                    // автоширина столбцов
                    worksheet.Columns.AutoFit();
                }

                // лист "Общий итог" после всех пользователей
                Excel.Worksheet summarySheet = workbook.Worksheets.Add(After: workbook.Worksheets[workbook.Worksheets.Count]);
                summarySheet.Name = "Общий итог";
                summarySheet.Cells[1, 1] = "Общий итог:";
                summarySheet.Cells[1, 2] = grandTotal;
                (summarySheet.Cells[1, 2] as Excel.Range).NumberFormat = "0.00";
                Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
                summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
                summaryRange.Font.Bold = true;
                summarySheet.Columns.AutoFit();

                // сохраняем в папку "Отчеты" рядом с программой, в имени файла дата и время,
                // чтобы не мешал прошлый отчет, если он еще открыт
                string folder = GetReportsFolder();
                string fileName = "Payments_" + DateTime.Now.ToString("dd.MM.yyyy_HH-mm-ss") + ".xlsx";
                // SaveCopyAs, потому что обычный SaveAs у Excel падает в папках OneDrive
                workbook.SaveCopyAs(System.IO.Path.Combine(folder, fileName));
                workbook.Saved = true;

                // показываем таблицу
                application.Visible = true;

                MessageBox.Show("Отчет сохранен:\n" + System.IO.Path.Combine(folder, fileName), "Экспорт в Excel");
            }
            catch (Exception ex)
            {
                // закрываем невидимый Excel, чтобы он не остался висеть в памяти
                if (application != null && !application.Visible)
                {
                    application.DisplayAlerts = false;
                    application.Quit();
                }
                MessageBox.Show("Ошибка при выгрузке в Excel:\n" + ex.Message, "Ошибка");
            }
        }

        // экспорт в Word
        private void BtnWord_Click(object sender, RoutedEventArgs e)
        {
            Word.Application application = null;
            try
            {
                // получаем список пользователей и категорий
                var allUsers = _context.User.ToList().OrderBy(u => u.FIO).ToList();
                var allCategories = _context.Category.ToList();

                // создаем новый документ Word
                application = new Word.Application();
                Word.Document document = application.Documents.Add();

                foreach (var user in allUsers)
                {
                    // ФИО пользователя - заголовок
                    Word.Paragraph userParagraph = document.Paragraphs.Add();
                    Word.Range userRange = userParagraph.Range;
                    userRange.Text = user.FIO;
                    userParagraph.set_Style(Word.WdBuiltinStyle.wdStyleTitle);
                    userRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    userRange.InsertParagraphAfter();
                    document.Paragraphs.Add(); // пустая строка

                    // таблица с платежами
                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table paymentsTable = document.Tables.Add(tableRange, allCategories.Count() + 1, 2);
                    paymentsTable.Borders.InsideLineStyle = paymentsTable.Borders.OutsideLineStyle =
                        Word.WdLineStyle.wdLineStyleSingle;
                    paymentsTable.Range.Cells.VerticalAlignment = Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    // шапка таблицы
                    Word.Range cellRange;
                    cellRange = paymentsTable.Cell(1, 1).Range;
                    cellRange.Text = "Категория";
                    cellRange = paymentsTable.Cell(1, 2).Range;
                    cellRange.Text = "Сумма расходов";

                    paymentsTable.Rows[1].Range.Font.Name = "Times New Roman";
                    paymentsTable.Rows[1].Range.Font.Size = 14;
                    paymentsTable.Rows[1].Range.Bold = 1;
                    paymentsTable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    // строки таблицы
                    for (int i = 0; i < allCategories.Count(); i++)
                    {
                        var currentCategory = allCategories[i];
                        cellRange = paymentsTable.Cell(i + 2, 1).Range;
                        cellRange.Text = currentCategory.Name;
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;

                        cellRange = paymentsTable.Cell(i + 2, 2).Range;
                        cellRange.Text = user.Payment.ToList()
                            .Where(u => u.CategoryID == currentCategory.ID)
                            .Sum(u => u.Num * u.Price).ToString("N2") + " руб.";
                        cellRange.Font.Name = "Times New Roman";
                        cellRange.Font.Size = 12;
                    }
                    document.Paragraphs.Add(); // пустая строка

                    // самый дорогой платеж (красным)
                    Payment maxPayment = user.Payment.OrderByDescending(u => u.Price * u.Num).FirstOrDefault();
                    if (maxPayment != null)
                    {
                        Word.Paragraph maxPaymentParagraph = document.Paragraphs.Add();
                        Word.Range maxPaymentRange = maxPaymentParagraph.Range;
                        maxPaymentRange.Text = $"Самый дорогостоящий платеж - {maxPayment.Name} за {(maxPayment.Price * maxPayment.Num).ToString("N2")} " +
                            $"руб. от {maxPayment.Date.ToString("dd.MM.yyyy")}";
                        maxPaymentParagraph.set_Style(Word.WdBuiltinStyle.wdStyleSubtitle);
                        maxPaymentRange.Font.Color = Word.WdColor.wdColorDarkRed;
                        maxPaymentRange.InsertParagraphAfter();
                    }
                    document.Paragraphs.Add(); // пустая строка

                    // самый дешевый платеж (зеленым)
                    Payment minPayment = user.Payment.OrderBy(u => u.Price * u.Num).FirstOrDefault();
                    if (minPayment != null)
                    {
                        Word.Paragraph minPaymentParagraph = document.Paragraphs.Add();
                        Word.Range minPaymentRange = minPaymentParagraph.Range;
                        minPaymentRange.Text = $"Самый дешевый платеж - {minPayment.Name} за {(minPayment.Price * minPayment.Num).ToString("N2")} " +
                            $"руб. от {minPayment.Date.ToString("dd.MM.yyyy")}";
                        minPaymentParagraph.set_Style(Word.WdBuiltinStyle.wdStyleSubtitle);
                        minPaymentRange.Font.Color = Word.WdColor.wdColorDarkGreen;
                        minPaymentRange.InsertParagraphAfter();
                    }

                    // каждый пользователь с новой страницы
                    if (user != allUsers.LastOrDefault())
                        document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                }

                foreach (Word.Section section in document.Sections)
                {
                    // нижний колонтитул - номер страницы по центру
                    Word.HeaderFooter footer = section.Footers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary];
                    footer.PageNumbers.Add(Word.WdPageNumberAlignment.wdAlignPageNumberCenter);

                    // верхний колонтитул - текущая дата
                    Word.Range headerRange = section.Headers[Word.WdHeaderFooterIndex.wdHeaderFooterPrimary].Range;
                    headerRange.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;
                    headerRange.Font.ColorIndex = Word.WdColorIndex.wdBlack;
                    headerRange.Font.Size = 10;
                    headerRange.Text = DateTime.Now.ToString("dd.MM.yyyy");
                }

                // показываем документ
                application.Visible = true;

                // сохраняем в .docx и .pdf в папку "Отчеты", в имени файла дата и время
                string folder = GetReportsFolder();
                string fileName = "Payments_" + DateTime.Now.ToString("dd.MM.yyyy_HH-mm-ss");
                document.SaveAs2(System.IO.Path.Combine(folder, fileName + ".docx"));
                document.ExportAsFixedFormat(System.IO.Path.Combine(folder, fileName + ".pdf"), Word.WdExportFormat.wdExportFormatPDF);

                MessageBox.Show("Отчет сохранен в папку:\n" + folder + "\n\n" + fileName + ".docx\n" + fileName + ".pdf", "Экспорт в Word");
            }
            catch (Exception ex)
            {
                // закрываем невидимый Word, чтобы он не остался висеть в памяти
                if (application != null && !application.Visible)
                    application.Quit(false);
                MessageBox.Show("Ошибка при выгрузке в Word:\n" + ex.Message, "Ошибка");
            }
        }
    }
}
