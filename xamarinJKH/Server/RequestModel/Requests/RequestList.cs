﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Xamarin.Forms;
using xamarinJKH.Utils;

namespace xamarinJKH.Server.RequestModel
{
    public class RequestList
    {
        public List<RequestInfo> Requests { get; set; }
        public string UpdateKey { get; set; }
        public string Error { get; set; }
    }
    
    public class RequestInfo
    {
        public int ID { get; set; }
        public int TypeID { get; set; }
        private int index = 0;
        public string RequestNumber { get; set; }
        public string Added { get; set; }
        
        public string RequestTerm { get; set; }

        public DateTime _RequestTerm
        {
            get => string.IsNullOrWhiteSpace(RequestTerm) ? DateTime.Now.AddDays(10).AddHours(index++) : DateTime.ParseExact(RequestTerm, "dd.MM.yyyy HH:mm:ss",
                System.Globalization.CultureInfo.CurrentCulture);
        }
// долг по лсч
        public decimal Debt { get; set; }
        public string _Added
        {
            get => Added.Trim();
            set => Added = value;
        }

        private bool isEnableMass = !Settings.MobileSettings.disableBulkRequestsClosing;
        

        public bool IsEnableMass
        {
            get => isEnableMass;
            set => isEnableMass = value;
        }
        
        public string Name { get; set; }
        public string Text  { get; set; }
        public string HalfName
        {
            get => Name.Length > 50 ? Settings.GetHalfAddress(Name) : Name;
            set => Name = value;
        }
        public string Status { get; set; }
        public int StatusID { get; set; }
        public bool IsClosed { get; set; }
        public bool IsCheked { get; set; } = false;
        
        public string Address { get; set; }
        public string Source { get; set; }

        private bool _isVisibleAddress;

        public bool IsVisibleAddress
        {
            get => !string.IsNullOrWhiteSpace(Address);
            set => _isVisibleAddress = value;
        }

        public bool IsPerformed { get; set; }
        public string PaidRequestStatus { get; set; } //- статус заказа
        public string PaidRequestCompleteCode { get; set; }//  - код подтверждения(подтягивается только для жителя)
        public bool IsPaidByUser { get; set; }
        public bool IsPaid { get; set; }
        public int ShopId { get; set; }
        // Флаг "Прочитана сотрудником"
        public bool IsReaded { get; set; }
        public bool IsReadedByClient { get; set; }
        // название приоритета
        public string PriorityName { get; set; }

        private Color _textColor;

        public Color TextColor
        {
            get => Settings.GetPriorityColor(PriorityId);
            set => _textColor = value;
        }

        // ид приоритета
        public int PriorityId { get; set; }
        public bool _IsReadedByClient
        {
            get=>!IsReadedByClient && StatusID != 6;
            set=> s = value;
        }

        private bool s;
        
        private string _resource { get; set; }

        public string Resource => "resource://xamarinJKH.Resources." + Settings.GetStatusIcon(StatusID) + ".svg";

        // источник заявки
        public string SourceType { get; set; }
        // тип неисправности
        public string MalfunctionType { get; set; }

        public string _MalfunctionType
        {
            get => string.IsNullOrWhiteSpace(MalfunctionType)? "" :  " (" + MalfunctionType + ")";
            set => MalfunctionType = value;
        }
        // исполнитель
        public string PerofmerName { get; set; }

        //public override string ToString()
        //{
        //    return $"Id={ID} isClosed={IsClosed} RequestNumber={RequestNumber}";
        //}
    }

    public class RequestContent : RequestInfo
    {
        public string Phone { get; set; }
        public string Address { get; set; }

        public string HalfAdress
        {
            get => !string.IsNullOrWhiteSpace(Address) && Address.Length > 50 ? Settings.GetHalfAddress(Address) : Address;
            set => Address = value;
        }
        public int TypeID { get; set; }
        public string TypeName { get; set; }
        public string AuthorName { get; set; }
        public List<RequestMessage> Messages { get; set; }
        public List<RequestFile> Files { get; set; }
        public List<RequestsReceiptItem> ReceiptItems { get; set; }
        public decimal PaidSumm { get; set; }
        public string PaidServiceText { get; set; }
        public string Error { get; set; }
        // информация о пропуске
        public RequestPass PassInfo { get; set; }
        public RequestContent Copy()
        {
            return (RequestContent)this.MemberwiseClone();
        }
    }

    public class RequestMessage
    {
        private string dateAdd;
        private string timeAdd;
        private bool dateVisible1;

        public int ID { get; set; }
        public string Added { get; set; }
        public string AuthorName { get; set; }
        public string Text { get; set; }
        public bool IsHidden  { get; set; }
        public int FileID { get; set; }
        // сообщение создано текущим пользователем
        public bool IsSelf { get; set; }
        public string DateAdd { get => Added.Split(' ')[0]; set => dateAdd = value; }
        public string TimeAdd { get => Added.Split(' ')[1].Substring(0, 5); set => timeAdd = value; }
        public bool dateVisible
        {
            get
            {
                if (Settings.DateUniq.Equals(DateAdd))
                    return false;
                else
                {
                    Settings.DateUniq = DateAdd;
                    return true;
                }

            }
            set => dateVisible1 = value;
        }
    }

    public class RequestFile
    {
        public int ID { get; set; }
        public string Added { get; set; }
        public string AuthorName { get; set; }
        public string Name { get; set; }
        // файл загружен текущим пользователем
        public bool IsSelf { get; set; }
        public int FileSize { get; set; }
    }

}