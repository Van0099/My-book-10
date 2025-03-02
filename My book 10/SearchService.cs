using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace My_book_10
{
    internal class SearchService
    {
        private RichTextBox _rtb;
        private TextBox _searchBox;
        private int _lastIndex = -1; // Индекс последнего найденного слова
        private List<TextRange> _foundRanges = new List<TextRange>(); // Все найденные совпадения
        private SolidColorBrush highlightBrush = new SolidColorBrush(Colors.Yellow); // Подсветка

        public SearchService(RichTextBox rtbEditor, TextBox searchBox)
        {
            _rtb = rtbEditor;
            _searchBox = searchBox;
        }

        // Очистка выделения перед поиском
        private void ClearHighlights()
        {
            foreach (var range in _foundRanges)
            {
                range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
            }
            _foundRanges.Clear();
        }

        // Основной метод поиска
        public void SearchText()
        {
            ClearHighlights();
            _lastIndex = -1; // Сброс последнего индекса

            if (string.IsNullOrWhiteSpace(_searchBox.Text)) return;

            TextRange docRange = new TextRange(_rtb.Document.ContentStart, _rtb.Document.ContentEnd);
            string text = docRange.Text;
            string searchText = _searchBox.Text;

            int startIndex = 0;
            while ((startIndex = text.IndexOf(searchText, startIndex, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                TextPointer start = GetTextPointerAtOffset(_rtb.Document.ContentStart, startIndex);
                TextPointer end = GetTextPointerAtOffset(start, searchText.Length);
                TextRange range = new TextRange(start, end);

                range.ApplyPropertyValue(TextElement.BackgroundProperty, highlightBrush);
                _foundRanges.Add(range);

                startIndex += searchText.Length;
            }
        }

        // Метод поиска вперед
        public void SearchNext()
        {
            if (_foundRanges.Count == 0) return;

            _lastIndex++;
            if (_lastIndex >= _foundRanges.Count) _lastIndex = 0;

            HighlightCurrent();
        }

        // Метод поиска назад
        public void SearchBack()
        {
            if (_foundRanges.Count == 0) return;

            _lastIndex--;
            if (_lastIndex < 0) _lastIndex = _foundRanges.Count - 1;

            HighlightCurrent();
        }

        // Подсветка текущего найденного элемента
        private void HighlightCurrent()
        {
            foreach (var range in _foundRanges)
            {
                range.ApplyPropertyValue(TextElement.BackgroundProperty, highlightBrush);
            }

            _foundRanges[_lastIndex].ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Orange);
            _rtb.Selection.Select(_foundRanges[_lastIndex].Start, _foundRanges[_lastIndex].End);
            _rtb.Focus();
        }

        // Получение TextPointer по индексу символа
        private TextPointer GetTextPointerAtOffset(TextPointer start, int offset)
        {
            TextPointer pointer = start;
            while (offset > 0 && pointer != null)
            {
                if (pointer.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    int count = pointer.GetTextRunLength(LogicalDirection.Forward);
                    if (offset <= count)
                        return pointer.GetPositionAtOffset(offset);
                    offset -= count;
                }
                pointer = pointer.GetNextContextPosition(LogicalDirection.Forward);
            }
            return pointer;
        }
    }
}
