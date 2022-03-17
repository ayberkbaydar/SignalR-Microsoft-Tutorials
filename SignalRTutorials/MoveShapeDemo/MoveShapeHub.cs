using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Threading;

namespace MoveShapeDemo
{
    public class Broadcaster
    {
        private readonly static Lazy<Broadcaster> _instance = new Lazy<Broadcaster>(() => new Broadcaster());
        private readonly TimeSpan BroadcastInterval = TimeSpan.FromMilliseconds(40);
        private readonly IHubContext _hubContext;
        private readonly Timer _broadcastLoop;
        private ShapeModel _model;
        private bool _modelUpdated;

        public Broadcaster()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MoveShapeHub>();
            _model = new ShapeModel();
            _modelUpdated = false;
            _broadcastLoop = new Timer(
                BroadcastShape,
                null,
                BroadcastInterval,
                BroadcastInterval);
        }

        public void BroadcastShape(object state)
        {
            if (_modelUpdated)
            {
                _hubContext.Clients.AllExcept(_model.LastUpdatedBy).updateShape(_model);
                _modelUpdated = false;
            }
        }
        public void UpdateShape(ShapeModel clientModel)
        {
            _model = clientModel;
            _modelUpdated = true;
        }
        public static Broadcaster Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }
    public class MoveShapeHub : Hub
    {
        public void UpdateModel(ShapeModel clientModel)
        {
            clientModel.LastUpdatedBy = Context.ConnectionId;
            Clients.AllExcept(clientModel.LastUpdatedBy).updateShape(clientModel);
        }
    }
    public class ShapeModel
    {
        [JsonProperty("left")]
        public double Left { get; set; }
        [JsonProperty("top")]
        public double Top { get; set; }
        [JsonIgnore]
        public string LastUpdatedBy { get; set; }
    }
}