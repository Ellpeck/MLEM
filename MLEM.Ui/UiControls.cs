using MLEM.Input;

namespace MLEM.Ui {
    public class UiControls {

        public BoolQuery MainButton = h => h.IsMouseButtonPressed(MouseButton.Left);
        public BoolQuery SecondaryButton = h => h.IsMouseButtonPressed(MouseButton.Right);
        public FloatQuery Scroll = h => h.LastScrollWheel - h.ScrollWheel;

        public delegate bool BoolQuery(InputHandler handler);

        public delegate float FloatQuery(InputHandler handler);

    }
}