using Gita.Practice.App.Models;
using System.Collections.ObjectModel;

namespace Gita.Practice.App.ViewModels
{
    public class StatusViewModel : BaseViewModel
    {
        public StatusViewModel()
        {
        }

        public string LastMessage { get; set; }
        public bool IsError { get; set; }
        public void UpdateLastMessage(string message)
        {
            this.LastMessage = message;
            if(message.ToLower().Contains("error"))
            {
                this.IsError = true;
            }
            else
            {
                this.IsError = false;
            }

            OnPropertyChanged(nameof(LastMessage));
            OnPropertyChanged(nameof(IsError));
        }
    }
}
