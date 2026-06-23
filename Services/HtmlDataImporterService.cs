#if FALSE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GIUA_KY_WEB_3AE.Data;
using GIUA_KY_WEB_3AE.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;

namespace GIUA_KY_WEB_3AE.Services
{
    /// <summary>
    /// Service hỗ trợ đọc các file HTML xuất từ Excel Bếp Nhà Xíu và import dữ liệu vào database SQLite.
    /// </summary>
    public class HtmlDataImporterService
    {
        private readonly ApplicationDbContext _context;

        public HtmlDataImporterService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Loại bỏ các dấu tiếng Việt khỏi chuỗi văn bản để phục vụ việc so sánh không dấu.
        /// </summary>
        /// <param name="text">Chuỗi văn bản cần loại bỏ dấu.</param>
        /// <returns>Chuỗi văn bản đã loại bỏ dấu.</returns>
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string normalized = text.Normalize(System.Text.NormalizationForm.FormD);
            var sb = new System.Text.StringBuilder();
            foreach (char c in normalized)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        }

        /// <summary>
        /// Tìm đường dẫn tệp tin trong thư mục dựa trên các từ khóa không dấu để tránh lỗi Mojibake.
        /// </summary>
        /// <param name="folderPath">Thư mục chứa các tệp tin.</param>
        /// <param name="fallbackName">Tên tệp tin dự phòng nếu không tìm thấy bằng từ khóa.</param>
        /// <param name="keywords">Danh sách các từ khóa cần tìm.</param>
        /// <returns>Đường dẫn tuyệt đối đến tệp tin tìm thấy.</returns>
        public static string FindFileByKeywords(string folderPath, string fallbackName, string[] keywords)
        {
            if (!Directory.Exists(folderPath))
            {
                return Path.Combine(folderPath, fallbackName);
            }

            var files = Directory.GetFiles(folderPath, "*.html");
            foreach (var file in files)
            {
                string fileName = Path.GetFileName(file);
                string cleanName = RemoveDiacritics(fileName).ToLower();

                bool isMatch = true;
                foreach (var kw in keywords)
                {
                    string cleanKw = RemoveDiacritics(kw).ToLower();
                    if (!cleanName.Contains(cleanKw))
                    {
                        isMatch = false;
                        break;
                    }
                }

                if (isMatch)
                {
                    return file;
                }
            }

            return Path.Combine(folderPath, fallbackName);
        }

        /// <summary>
        /// Import toàn bộ dữ liệu từ các file HTML trong thư mục chỉ định.
        /// </summary>
        /// <param name="folderPath">Đường dẫn tuyệt đối đến thư mục chứa các file HTML.</param>
        public async Task ImportAllAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                throw new DirectoryNotFoundException($"Thư mục '{folderPath}' không tồn tại.");
            }

            // 1. Import khách hàng
            string khachHangPath = FindFileByKeywords(folderPath, "Khách hàng.html", new[] { "khach", "hang" });
            if (File.Exists(khachHangPath))
            {
                await ImportCustomersAsync(khachHangPath);
            }

            // 2. Import sản phẩm, nguyên liệu và định lượng công thức
            string sanPhamPath = FindFileByKeywords(folderPath, "Sản phẩm.html", new[] { "san", "pham" });
            if (File.Exists(sanPhamPath))
            {
                await ImportProductsAndRecipesAsync(sanPhamPath);
            }

            // 3. Import giao dịch thu chi
            string thuChiPath = FindFileByKeywords(folderPath, "THU, CHI.html", new[] { "thu", "chi" });
            if (File.Exists(thuChiPath))
            {
                await ImportTransactionsAsync(thuChiPath);
            }

            // 4. Import báo cáo tháng
            string quanLyPath = FindFileByKeywords(folderPath, "Quản lý.html", new[] { "quan", "ly" });
            if (File.Exists(quanLyPath))
            {
                await ImportMonthlySnapshotsAsync(quanLyPath);
            }

            // 5. Import sổ công nợ
            string congNoPath = FindFileByKeywords(folderPath, "Trang tính8.html", new[] { "trang", "tinh", "8" });
            if (File.Exists(congNoPath))
            {
                await ImportDebtLedgersAsync(congNoPath);
            }
        }

        /// <summary>
        /// Parse và import dữ liệu khách hàng từ Khách hàng.html.
        /// </summary>
        public async Task ImportCustomersAsync(string filePath)
        {
            var doc = new HtmlDocument();
            doc.Load(filePath);

            var rows = doc.DocumentNode.SelectNodes("//table/tbody/tr");
            if (rows == null) return;

            var customers = new List<Customer>();

            // Duyệt từ hàng thứ 3 (index 2) trở đi
            for (int i = 2; i < rows.Count; i++)
            {
                var cols = rows[i].SelectNodes("td");
                if (cols == null || cols.Count < 6) continue;

                string name = CleanText(cols[1].InnerText);
                if (string.IsNullOrEmpty(name)) continue;

                string phone = CleanText(cols[2].InnerText);
                string address = CleanText(cols[3].InnerText);
                string ward = CleanText(cols[4].InnerText);
                string district = CleanText(cols[5].InnerText);

                customers.Add(new Customer
                {
                    Name = name,
                    PhoneNumber = phone,
                    Address = address,
                    Ward = ward,
                    District = district,
                    Area = ward // Tạm lấy Ward làm Area
                });
            }

            if (customers.Any())
            {
                // Xóa dữ liệu khách hàng cũ để tránh trùng lặp khi import lại
                _context.Customers.RemoveRange(_context.Customers);
                await _context.Customers.AddRangeAsync(customers);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Parse và import dữ liệu sản phẩm, nguyên liệu và công thức từ Sản phẩm.html.
        /// </summary>
        public async Task ImportProductsAndRecipesAsync(string filePath)
        {
            var doc = new HtmlDocument();
            doc.Load(filePath);

            var rows = doc.DocumentNode.SelectNodes("//table/tbody/tr");
            if (rows == null || rows.Count < 5) return;

            // Xóa dữ liệu cũ liên quan
            _context.RecipeRequirements.RemoveRange(_context.RecipeRequirements);
            _context.Ingredients.RemoveRange(_context.Ingredients);
            _context.SellableItems.RemoveRange(_context.SellableItems);
            await _context.SaveChangesAsync();

            // 1. Tìm hàng tiêu đề chứa cột "Tên sản phẩm" ở cột B (index 1) hoặc cột bất kỳ để lấy header động
            int headerRowIndex = -1;
            for (int i = 0; i < rows.Count; i++)
            {
                var cols = rows[i].SelectNodes("td");
                if (cols != null && cols.Count > 1)
                {
                    string col1Text = CleanText(cols[1].InnerText);
                    if (col1Text == "Tên sản phẩm" || col1Text == "Tên")
                    {
                        headerRowIndex = i;
                        break;
                    }
                }
            }

            if (headerRowIndex == -1)
            {
                headerRowIndex = 3; // Fallback
            }

            var headerCols = rows[headerRowIndex].SelectNodes("td");
            if (headerCols == null || headerCols.Count < 7) return;

            var sellableItems = new List<SellableItem>();
            var sellableItemColIndices = new List<int>();

            // Lấy giá trị lợi nhuận (dòng mốc - 3), giá bán (dòng mốc - 2), giá vốn (dòng mốc - 1)
            HtmlNodeCollection? profitCols = headerRowIndex >= 3 ? rows[headerRowIndex - 3].SelectNodes("td") : null;
            HtmlNodeCollection? priceCols = headerRowIndex >= 2 ? rows[headerRowIndex - 2].SelectNodes("td") : null;
            HtmlNodeCollection? costCols = headerRowIndex >= 1 ? rows[headerRowIndex - 1].SelectNodes("td") : null;

            for (int colIndex = 6; colIndex < headerCols.Count; colIndex++)
            {
                string itemName = CleanText(headerCols[colIndex].InnerText);
                if (string.IsNullOrEmpty(itemName)) continue;

                decimal sellingPrice = (priceCols != null && colIndex < priceCols.Count) ? ParseDecimal(priceCols[colIndex].InnerText) : 0;
                decimal costPrice = (costCols != null && colIndex < costCols.Count) ? ParseDecimal(costCols[colIndex].InnerText) : 0;
                decimal estimatedProfit = (profitCols != null && colIndex < profitCols.Count) ? ParseDecimal(profitCols[colIndex].InnerText) : 0;

                var item = new SellableItem
                {
                    Name = itemName,
                    SellingPrice = sellingPrice,
                    CostPrice = costPrice,
                    EstimatedProfit = estimatedProfit
                };
                sellableItems.Add(item);
                sellableItemColIndices.Add(colIndex);
            }

            if (sellableItems.Any())
            {
                await _context.SellableItems.AddRangeAsync(sellableItems);
                await _context.SaveChangesAsync();
            }

            // 2. Phân tích Ingredients từ các hàng sau headerRowIndex trở đi
            var ingredients = new List<Ingredient>();
            var ingredientRows = new List<HtmlNode>();

            for (int r = headerRowIndex + 1; r < rows.Count; r++)
            {
                var cols = rows[r].SelectNodes("td");
                if (cols == null || cols.Count < 6) continue;

                // Bỏ qua dòng freezebar hoặc dòng trống phân cách
                if (rows[r].Attributes["class"]?.Value?.Contains("freezebar-cell") == true || 
                    rows[r].InnerHtml.Contains("freezebar-cell"))
                {
                    continue;
                }

                string ingName = CleanText(cols[1].InnerText);
                if (string.IsNullOrEmpty(ingName)) continue;

                string spec = CleanText(cols[2].InnerText);
                decimal importPrice = ParseDecimal(cols[3].InnerText);
                decimal packageQty = ParseDecimal(cols[4].InnerText);
                decimal unitPrice = ParseDecimal(cols[5].InnerText);

                // Phòng trường hợp PackageQuantity = 0
                if (packageQty == 0) packageQty = 1;
                if (unitPrice == 0 && importPrice > 0) unitPrice = importPrice / packageQty;

                var ingredient = new Ingredient
                {
                    Name = ingName,
                    Specification = spec,
                    ImportPrice = importPrice,
                    PackageQuantity = packageQty,
                    UnitPrice = unitPrice
                };
                ingredients.Add(ingredient);
                ingredientRows.Add(rows[r]);
            }

            if (ingredients.Any())
            {
                await _context.Ingredients.AddRangeAsync(ingredients);
                await _context.SaveChangesAsync();
            }

            // 3. Phân tích Công thức (Recipe requirements)
            var recipeReqs = new List<RecipeRequirement>();

            for (int i = 0; i < ingredients.Count; i++)
            {
                var ing = ingredients[i];
                var rowNode = ingredientRows[i];
                var cols = rowNode.SelectNodes("td");

                for (int j = 0; j < sellableItems.Count; j++)
                {
                    var item = sellableItems[j];
                    int colIdx = sellableItemColIndices[j];

                    if (colIdx < cols.Count)
                    {
                        string valStr = CleanText(cols[colIdx].InnerText);
                        if (!string.IsNullOrEmpty(valStr))
                        {
                            decimal qtyUsed = ParseDecimal(valStr);
                            if (qtyUsed > 0)
                            {
                                recipeReqs.Add(new RecipeRequirement
                                {
                                    IngredientId = ing.Id,
                                    SellableItemId = item.Id,
                                    QuantityUsed = qtyUsed
                                });
                            }
                        }
                    }
                }
            }

            if (recipeReqs.Any())
            {
                await _context.RecipeRequirements.AddRangeAsync(recipeReqs);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Parse và import dữ liệu giao dịch thu chi từ THU, CHI.html.
        /// </summary>
        public async Task ImportTransactionsAsync(string filePath)
        {
            var doc = new HtmlDocument();
            doc.Load(filePath);

            var rows = doc.DocumentNode.SelectNodes("//table/tbody/tr");
            if (rows == null || rows.Count < 4) return;

            // Xóa dữ liệu cũ
            _context.Transactions.RemoveRange(_context.Transactions);
            await _context.SaveChangesAsync();

            var transactions = new List<Transaction>();

            // Hàng tiêu đề ở hàng index 2 (Row 3). Dữ liệu bắt đầu từ hàng index 3.
            for (int i = 3; i < rows.Count; i++)
            {
                var cols = rows[i].SelectNodes("td");
                if (cols == null || cols.Count < 10) continue;

                string yearStr = CleanText(cols[1].InnerText);
                if (string.IsNullOrEmpty(yearStr) || !int.TryParse(yearStr, out int year)) continue;

                int month = int.TryParse(CleanText(cols[2].InnerText), out int m) ? m : 1;
                int day = int.TryParse(CleanText(cols[3].InnerText), out int d) ? d : 1;

                // Tự động phát hiện lệch cột do thanh freezebar-cell đóng băng tại cột E (index 4)
                int typeIdx = 4;
                int contentIdx = 5;
                int amountIdx = 6;
                int orderIdx = 7;
                int catIdx = 8;
                int noteIdx = 9;

                string testCol4 = CleanText(cols[4].InnerText);
                string testCol5 = CleanText(cols[5].InnerText);
                bool isShifted = cols[4].Attributes["class"]?.Value?.Contains("freezebar") == true || 
                                 (string.IsNullOrEmpty(testCol4) && (testCol5 == "THU" || testCol5 == "CHI"));

                if (isShifted && cols.Count >= 11)
                {
                    typeIdx = 5;
                    contentIdx = 6;
                    amountIdx = 7;
                    orderIdx = 8;
                    catIdx = 9;
                    noteIdx = 10;
                }

                string type = CleanText(cols[typeIdx].InnerText); // "THU" hoặc "CHI"
                string content = CleanText(cols[contentIdx].InnerText);
                decimal amount = ParseDecimal(cols[amountIdx].InnerText);
                int orderCount = int.TryParse(CleanText(cols[orderIdx].InnerText), out int oc) ? oc : 0;
                string category = CleanText(cols[catIdx].InnerText);
                string note = CleanText(cols[noteIdx].InnerText);

                if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(content)) continue;

                transactions.Add(new Transaction
                {
                    Year = year,
                    Month = month,
                    Day = day,
                    Type = type,
                    Content = content,
                    Amount = Math.Abs(amount), // Thống nhất lưu số dương tuyệt đối
                    OrderCount = orderCount,
                    Category = category,
                    Note = note
                });
            }

            if (transactions.Any())
            {
                // Chia nhỏ danh sách lưu để tránh bị lỗi quá tải SQLite parameter
                int batchSize = 1000;
                for (int i = 0; i < transactions.Count; i += batchSize)
                {
                    var batch = transactions.Skip(i).Take(batchSize);
                    await _context.Transactions.AddRangeAsync(batch);
                    await _context.SaveChangesAsync();
                }
            }
        }

        /// <summary>
        /// Parse và import báo cáo tháng từ Quản lý.html.
        /// </summary>
        public async Task ImportMonthlySnapshotsAsync(string filePath)
        {
            var doc = new HtmlDocument();
            doc.Load(filePath);

            var rows = doc.DocumentNode.SelectNodes("//table/tbody/tr");
            if (rows == null || rows.Count < 4) return;

            // Xóa dữ liệu cũ
            _context.MonthlySnapshots.RemoveRange(_context.MonthlySnapshots);
            await _context.SaveChangesAsync();

            var snapshots = new List<MonthlySnapshot>();

            // Bỏ qua hàng 2 (index 1) vì là dòng chứa Tổng (Totals)
            // Tiêu đề ở hàng 3 (index 2). Dữ liệu bắt đầu từ hàng 4 (index 3)
            for (int i = 3; i < rows.Count; i++)
            {
                var cols = rows[i].SelectNodes("td");
                if (cols == null || cols.Count < 7) continue;

                string yearStr = CleanText(cols[1].InnerText);
                if (string.IsNullOrEmpty(yearStr) || !int.TryParse(yearStr, out int year)) continue;

                string monthStr = CleanText(cols[2].InnerText);
                if (string.IsNullOrEmpty(monthStr) || !int.TryParse(monthStr, out int month)) continue;

                int orderCount = int.TryParse(CleanText(cols[3].InnerText), out int oc) ? oc : 0;
                decimal income = ParseDecimal(cols[4].InnerText);
                decimal expense = ParseDecimal(cols[5].InnerText);
                decimal profit = ParseDecimal(cols[6].InnerText);

                snapshots.Add(new MonthlySnapshot
                {
                    Year = year,
                    Month = month,
                    OrderCount = orderCount,
                    TotalIncome = income,
                    TotalExpense = Math.Abs(expense), // Lưu dạng dương
                    NetProfit = profit
                });
            }

            if (snapshots.Any())
            {
                await _context.MonthlySnapshots.AddRangeAsync(snapshots);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Parse và import công nợ hụi/họ từ Trang tính8.html.
        /// </summary>
        public async Task ImportDebtLedgersAsync(string filePath)
        {
            var doc = new HtmlDocument();
            doc.Load(filePath);

            var rows = doc.DocumentNode.SelectNodes("//table/tbody/tr");
            if (rows == null || rows.Count < 5) return;

            // Xóa công nợ cũ
            _context.DebtEntries.RemoveRange(_context.DebtEntries);
            _context.DebtLedgers.RemoveRange(_context.DebtLedgers);
            await _context.SaveChangesAsync();

            var ledgers = new List<DebtLedger>();
            var ledgerColIndices = new List<int>();

            // 1. Tìm hàng tiêu đề chứa cột "Kỳ" ở cột B (index 1)
            int headerRowIndex = -1;
            for (int i = 0; i < rows.Count; i++)
            {
                var cols = rows[i].SelectNodes("td");
                if (cols != null && cols.Count > 1)
                {
                    string col1Text = CleanText(cols[1].InnerText);
                    if (col1Text == "Kỳ")
                    {
                        headerRowIndex = i;
                        break;
                    }
                }
            }

            if (headerRowIndex == -1)
            {
                headerRowIndex = 3; // Fallback
            }

            var nameCols = rows[headerRowIndex].SelectNodes("td");
            if (nameCols == null) return;

            // Lấy khoảng thời gian (dòng mốc - 2) và tổng tiền (dòng mốc - 1)
            HtmlNodeCollection? rangeCols = headerRowIndex >= 2 ? rows[headerRowIndex - 2].SelectNodes("td") : null;
            HtmlNodeCollection? totalCols = headerRowIndex >= 1 ? rows[headerRowIndex - 1].SelectNodes("td") : null;

            // Các cột công nợ bắt đầu từ cột C (index 2) đến cuối bảng
            for (int colIndex = 2; colIndex < nameCols.Count; colIndex++)
            {
                string name = CleanText(nameCols[colIndex].InnerText);
                if (string.IsNullOrEmpty(name)) continue;

                string? periodRange = (rangeCols != null && colIndex < rangeCols.Count) ? CleanText(rangeCols[colIndex].InnerText) : "";
                decimal totalAmt = (totalCols != null && colIndex < totalCols.Count) ? ParseDecimal(totalCols[colIndex].InnerText) : 0;

                var ledger = new DebtLedger
                {
                    Name = name,
                    PeriodRange = periodRange,
                    TotalAmount = totalAmt,
                    Description = $"Dây hụi/nợ: {name}"
                };

                ledgers.Add(ledger);
                ledgerColIndices.Add(colIndex);
            }

            if (ledgers.Any())
            {
                await _context.DebtLedgers.AddRangeAsync(ledgers);
                await _context.SaveChangesAsync();
            }

            // 2. Đọc các kỳ thanh toán từ các hàng sau hàng tiêu đề
            var entries = new List<DebtEntry>();

            for (int r = headerRowIndex + 1; r < rows.Count; r++)
            {
                var cols = rows[r].SelectNodes("td");
                if (cols == null || cols.Count < 2) continue;

                // Bỏ qua dòng freezebar hoặc dòng trống phân cách
                if (rows[r].Attributes["class"]?.Value?.Contains("freezebar-cell") == true || 
                    rows[r].InnerHtml.Contains("freezebar-cell"))
                {
                    continue;
                }

                string periodStr = CleanText(cols[1].InnerText); // Cột B (index 1) là số kỳ
                if (string.IsNullOrEmpty(periodStr) || !int.TryParse(periodStr, out int periodNum)) continue;

                for (int j = 0; j < ledgers.Count; j++)
                {
                    var ledger = ledgers[j];
                    int colIdx = ledgerColIndices[j];

                    if (colIdx < cols.Count)
                    {
                        string amtStr = CleanText(cols[colIdx].InnerText);
                        if (!string.IsNullOrEmpty(amtStr))
                        {
                            decimal amt = ParseDecimal(amtStr);
                            entries.Add(new DebtEntry
                            {
                                DebtLedgerId = ledger.Id,
                                PeriodNumber = periodNum,
                                Amount = amt,
                                Status = "Đã thanh toán"
                            });
                        }
                        else
                        {
                            // Nếu ô trống, tạo một kỳ chờ thanh toán với số tiền bằng 0
                            entries.Add(new DebtEntry
                            {
                                DebtLedgerId = ledger.Id,
                                PeriodNumber = periodNum,
                                Amount = 0,
                                Status = "Chờ"
                            });
                        }
                    }
                }
            }

            if (entries.Any())
            {
                await _context.DebtEntries.AddRangeAsync(entries);
                await _context.SaveChangesAsync();
            }
        }

        private string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            // Decode HTML Entities, thay thế ký tự nbsp, khoảng trắng dư thừa
            string decoded = HtmlEntity.DeEntitize(text);
            return decoded.Replace("\u00a0", " ").Replace("&nbsp;", " ").Trim();
        }

        private decimal ParseDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            text = CleanText(text).Replace(" ", "").Replace("\u00a0", "");
            if (string.IsNullOrWhiteSpace(text)) return 0;

            // Nếu chuỗi chứa dấu chấm phân cách hàng nghìn (ví dụ 30.000 hoặc -742.695)
            if (text.Contains('.'))
            {
                text = text.Replace(".", "");
            }
            // Thay dấu phẩy thập phân thành dấu chấm để parse Invariant
            text = text.Replace(",", ".");

            if (decimal.TryParse(text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal val))
            {
                return val;
            }
            return 0;
        }
    }
}
#endif
