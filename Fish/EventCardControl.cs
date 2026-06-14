using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Fish
{
    public partial class EventCardControl : UserControl
    {
        public EventCardControl()
        {
            InitializeComponent();

            // Чтобы клик по любому внутреннему тексту или элементу на карточке 
            // вызывал событие клика самой карточки (для вызова бокового меню)
            foreach (Control c in this.Controls)
            {
                c.Click += (s, e) => this.OnClick(e);
            }
        }

        /// <summary>
        /// Базовый метод заполнения карточки
        /// </summary>
        public void SetData(string name, string venue, string time, decimal minPrice)
        {
            lblName.Text = name;
            lblVenue.Text = venue;
            lblTime.Text = time;
            lblPrice.Text = $"от {minPrice:N0} р.";

            toolTipInfo.SetToolTip(lblName, name);
            toolTipInfo.SetToolTip(lblVenue, venue);
        }

        /// <summary>
        /// Адаптированный метод для Кинорелизов (Группированных)
        /// </summary>
        public void SetDataForMovie(string title, int cinemaCount, decimal minPrice)
        {
            lblName.Text = title;
            lblVenue.Text = "Сеансы в кинотеатрах"; // Поясняющий текст вместо конкретного зала
            lblTime.Text = $"Кинотеатров: {cinemaCount}"; // Вместо времени выводим количество площадок
            lblPrice.Text = $"от {minPrice:N0} руб.";

            toolTipInfo.SetToolTip(lblName, title);
        }

        /// <summary>
        /// Адаптированный метод для обычных Концертов/Мероприятий
        /// </summary>
        public void SetDataForEvent(string title, string venue, string time, decimal minPrice)
        {
            lblName.Text = title;
            lblVenue.Text = venue; // площадка
            lblTime.Text = time;   // Точное время начала (например, 20:00)
            lblPrice.Text = $"от {minPrice:N0} руб.";

            toolTipInfo.SetToolTip(lblName, title);
            toolTipInfo.SetToolTip(lblVenue, venue);
        }

        /// <summary>
        /// Метод чередования цветов (Зебра) с обязательным контролем отображения шрифтов
        /// </summary>
        public void SetRowIndex(int index)
        {
            // Устанавливаем четкий шрифт для элементов карточки, чтобы избежать багов невидимого текста
            Font regularFont = new Font("Arial", 10, FontStyle.Regular);
            Font boldFont = new Font("Arial", 12, FontStyle.Bold);

            lblName.Font = boldFont;
            lblVenue.Font = regularFont;
            lblTime.Font = regularFont;
            lblPrice.Font = boldFont;

            // Чередуем фоновые цвета карточки
            if (index % 4 == 0 || index % 4 == 3)
            {
                this.BackColor = Color.White;
            }
            else
            {
                this.BackColor = Color.FromArgb(245, 247, 250);
            }

            lblName.ForeColor = Color.Black;
            lblVenue.ForeColor = Color.Gray;
            lblTime.ForeColor = Color.DarkSlateGray;
            lblPrice.ForeColor = Color.Crimson;
        }
    }
}