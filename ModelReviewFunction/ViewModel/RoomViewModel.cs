using ModelReviewFunction.MVVM;

namespace ModelReviewFunction.ViewModel
{
    public class RoomViewModel : MyObservableObject
    {
        /// <summary>
        /// 房间名称
        /// </summary>
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; RaisePropertyChanged(() => Name); }
        }


        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set { isChecked = value; RaisePropertyChanged(() => IsChecked); }
        }

        private int roomId;
        public int RoomId
        {
            get { return roomId; }
            set { roomId = value; RaisePropertyChanged(() => RoomId); }
        }
    }
}
