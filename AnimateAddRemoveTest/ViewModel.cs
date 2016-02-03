using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AnimateAddRemoveTest
{
    class ViewModel : ViewModelBase
    {
        private ObservableCollection<Person> people = new ObservableCollection<Person>();

        public ObservableCollection<Person> People
        {
            get { return people; }
            set { people = value; OnPropertyChanged(nameof(People)); }
        }

        public ICommand AddPersonCommand { get; set; }
        public ICommand InsertPersonCommand { get; set; }
        public ICommand RemovePersonCommand { get; private set; }

        public ViewModel()
        {
            AddPersonCommand = new RelayCommand(addPerson);
            InsertPersonCommand = new RelayCommand(insertPerson, canInsertPerson);
            RemovePersonCommand = new RelayCommand(removePerson);
            for (int i = 0; i <= 0; i++)
            {
                //people.Add(new Person() { FirstName = "Fred", LastName = "Flintstone", Age = 37 });
                //people.Add(new Person() { FirstName = "Barney", LastName = "Rubble", Age = 62 });
                //people.Add(new Person() { FirstName = "Wilma", LastName = "Brown", Age = 25 });
            }
        }

        void addPerson(object o)
        {
            Person p = new Person { FirstName = "James", LastName = "Wood", Age = 11 };
            people.Add(p);
        }

        bool canInsertPerson(object o)
        {
            return people.Contains((Person)o);
        }

        void insertPerson(object o)
        {
            int pos = people.IndexOf((Person)o);
            Person p = new Person { FirstName = "Catherine", LastName = "Connerly", Age = 50 };
            people.Insert(pos, p);
        }

        void removePerson(object o)
        {
            people.Remove((Person)o);
        }
    }

    class Person : ViewModelBase
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        bool isRemoving;
        public bool IsRemoving
        {
            get { return isRemoving; }
            set
            {
                isRemoving = value;
                OnPropertyChanged(nameof(IsRemoving));
            }
        }
    }
}
