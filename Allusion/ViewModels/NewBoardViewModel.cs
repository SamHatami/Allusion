﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Allusion.WPFCore.Events;
using Caliburn.Micro;

namespace Allusion.ViewModels
{
    public class NewBoardViewModel : Screen
    {
        private IEventAggregator _events;

        private string _newBoardName;

        public string NewBoardName
        {
            get => _newBoardName;
            set
            {
                _newBoardName = value;
                NotifyOfPropertyChange(nameof(NewBoardName));
            }
        }
        public NewBoardViewModel(IEventAggregator events)
        {
            _events = events;
        }

        public Task Ok()
        {
            if (string.IsNullOrEmpty(NewBoardName))
                return Task.CompletedTask;

            _events.PublishOnBackgroundThreadAsync(new NewBoardEvent(NewBoardName));

            return TryCloseAsync(true);
        }

        public Task Cancel()
        {
            return TryCloseAsync(false);
        }
    }
}
