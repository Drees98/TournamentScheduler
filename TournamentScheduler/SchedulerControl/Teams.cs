using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TournamentScheduler.Common;

//ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

namespace TournamentScheduler.SchedulerControl
{
    class Teams : ItemBase
    {

		private int _count = 0;
		public int Count
		{
			get => _count;
			set
			{
				if (value != _count)
				{
					_count = value;
					OnPropertyChanged(nameof(Count));
				}
			}
		}


		private int _byesCount;
		public int ByesCount
		{
			get => _byesCount;
			set
			{
				if (value != _byesCount)
				{
					_byesCount = value;
					OnPropertyChanged(nameof(ByesCount));
				}
			}
		}


		private static ObservableCollection<Team> _teamsList = new ObservableCollection<Team>();
		public ReadOnlyObservableCollection<Team> TeamsList { get; private set; }

		private static ObservableCollection<Team> _byesList = new ObservableCollection<Team>();
		public ReadOnlyObservableCollection<Team> ByesList { get; private set; }

		public ICommand AddTeamCommand { get; private set; }
		public ICommand RemoveTeamCommand { get; private set; }
		public ICommand GenerateTournamentCommand { get; private set; }

		private void SetCommands()
		{
			AddTeamCommand = new RelayCommand<object>(x => {
				if(string.IsNullOrWhiteSpace(x.ToString())) return;
				Team team = new Team(x.ToString());
				if (!_teamsList.Any(x => x.Name == team.Name))
				{
					_teamsList.Add(team);
					Count++;
					ByesCount = CalculateByes(_count);
                    SetByesList();
                }
			});

			RemoveTeamCommand = new RelayCommand<Team>(x => {
				_teamsList.Remove(x);
				Count = _teamsList.Count;
                ByesCount = CalculateByes(_count);
				SetByesList();
            });

			GenerateTournamentCommand = new RelayCommand<object>(x =>
			{
                Random rng = new Random();
				List<Team> teams = _teamsList.ToList().OrderBy(_ =>rng.Next()).ToList();
				GenerateTournament(teams, "Test.xlsx");
			});
		}

		private int CalculateByes(int count)
		{
			int p2 = 2;
			while (count > p2)
			{
				p2 <<= 1;
			}

			return count <= 2 ? 0 : p2 - count;
		}

		private void SetByesList()
        {
            _byesList.Clear();
            for (int i = 0; i < ByesCount; i++)
            {
                _byesList.Add(_teamsList[i]);
            }
        }

		private void GenerateTournament(List<Team> teams, string filename)
		{
			if (!filename.EndsWith(".xlsx")) filename += ".xlsx";
			ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
			var p = new ExcelPackage(new System.IO.FileInfo(filename));
			var ws = p.Workbook.Worksheets.Add("Tournament");
			char x = 'A';
			int prevMax = 0;
			int y = 1;
			int r2Teams = ((teams.Count - _byesCount) / 2);
			int remainingGames = (r2Teams + _byesCount) / 2; 
            if (_byesCount > r2Teams) y = ((_byesCount - r2Teams) * 2) + 2;
			else if (_byesCount > 0) y = 2;

            for (int i = _byesCount; i < teams.Count; i++) 
			{
				string pos = x.ToString() + y.ToString();
				ws.Cells[pos].Value = teams[i].Name;
				y += 2;
			}
			prevMax = y;

			x++;

			if(_byesCount > 0)
            {
                y = 1;
				bool team = true;
				for(int i = 0; i < _byesCount; i++)
				{
					string pos = x.ToString() + y.ToString();
					if(_byesCount - i <= r2Teams)
					{
						if (team)
						{
							ws.Cells[pos].Value = teams[i].Name;
							team = !team;
						} 
						else
						{
							ws.Cells[pos].Style.Fill.PatternType = ExcelFillStyle.Solid;
							ws.Cells[pos].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
							i--;
							team = !team;
						}
					}
					else
                    {
                        ws.Cells[pos].Value = teams[i].Name;
                    }

					y += 2;
				}
			}

			p.Save();
		}

        public Teams()
		{
			TeamsList = new ReadOnlyObservableCollection<Team>(_teamsList);
			ByesList = new ReadOnlyObservableCollection<Team>(_byesList);
			SetCommands();
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
