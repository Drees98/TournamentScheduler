using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TournamentScheduler.SchedulerControl
{
    class Teams
    {
		private static ObservableCollection<Team> _teamsList = new ObservableCollection<Team>{new Team("Team1"), new Team("Team2") };
		public ReadOnlyObservableCollection<Team> TeamsList { get; private set; }

		public Teams()
		{
			TeamsList = new ReadOnlyObservableCollection<Team>(_teamsList);
		}
	}

	class Team : ItemBase
	{
		private string _name;
		public string Name { 
			get => _name;
			set
			{
				if (value != _name) 
				{
					_name = value;
					OnPropertyChanged(nameof(Name));
				}
			} 
		}

		public Team(string name)
		{
			_name = name;
		}
	}
}
