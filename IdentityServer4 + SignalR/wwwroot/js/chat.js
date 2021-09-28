$(document).ready(function () {
    var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

    connection.start().then(function () {
        console.log('SignalR Started...');

        // lần đầu load room, user:
        viewModel.roomList();
        viewModel.userList();
    });

    // khi add mới sẽ load tên room luôn 18:45 b5
    // sơ đồ load new room: view request create new room -> controller call "addChatRoom" -> chat.js -> view
    connection.on("addChatRoom", function (room) {
        viewModel.roomAdded(new ChatRoom(room.id, room.name));
    });

    function AppViewModel() { // có tác dụng binding dữ liệu trong model này cho view index
        var self = this;
        self.message = ko.observable(""); // tự động cập nhật dữ liệu, mặc định là rỗng
        self.chatRooms = ko.observableArray([]); // list rooms
        self.chatUsers = ko.observableArray([]); // list users
        self.chatMessages = ko.observableArray([]); // list messages
        self.joinedRoom = ko.observable("");
        self.joinedRoomId = ko.observable(""); // id của Hub mà ta tạo ra
        self.myName = ko.observable("");
        self.myAvatar = ko.observable("avatar1.png"); // mặc định là avatar 1
        self.isLoading = ko.observable(true); // khi lần đầu load có show lên web chat ko

        // enter -> send mess
        self.onEnter = function (d, e) {
            if (e.keyCode === 13) {
                self.sendNewMessage();
            }
            return true;
        }
        self.sendNewMessage = function () {
            var text = self.message();
            //send private
            if (text.startsWith("/")) {
                var receiver = text.substring(text.indexOf("(") + 1, text.indexOf(")"));
                var message = text.substring(text.indexOf(")") + 1, text.length);
                self.sendPrivate(receiver, message);
            }
            else {
                self.sendToRoom(self.joinedRoom(), self.message());
            }

            self.message("");
        }

        // filter user
        self.filter = ko.observable("");
        self.filteredChatUsers = ko.computed(function () {
            if (!self.filter()) {
                return self.chatUsers();
            } else {
                return ko.utils.arrayFilter(self.chatUsers(), function (user) {
                    var displayName = user.displayName().toLowerCase();
                    return displayName.includes(self.filter().toLowerCase());
                });
            }
        });

        self.joinRoom = function (room) {
            connection.invoke("Join", room.name()).then(function () {
                self.joinedRoom(room.name());
                self.joinedRoomId(room.id());
                self.userList();
                self.messageHistory();
            });
        }

        self.roomList = function () {
            fetch('/api/Rooms')
                .then(response => response.json())
                .then(data => {
                    self.chatRooms.removeAll();
                    for (var i = 0; i < data.length; i++) {
                        self.chatRooms.push(new ChatRoom(data[i].id, data[i].name));
                    }

                    if (self.chatRooms().length > 0)
                        self.joinRoom(self.chatRooms()[0]);
                });
        }

        self.createRoom = function () {
            var roomName = $("#roomName").val();
            fetch('/api/Rooms', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Name: roomName }),
            });
        }

        self.userList = function () {
            connection.invoke("GetUsers", self.joinedRoom()).then(function (result) {
                self.chatUsers.removeAll();
                for (var i = 0; i < result.length; i++) {
                    self.chatUsers.push(new ChatUser(result[i].username,
                        result[i].fullName,
                        result[i].avatar == null ? "default-avatar.png" : result[i].avatar,
                        result[i].currentRoom,
                        result[i].device))
                }
            });
        }

        self.roomAdded = function (room) {
            self.chatRooms.push(room);
        }
    }

    function ChatRoom(id, name) {
        var self = this;
        self.id = ko.observable(id);
        self.name = ko.observable(name);
    }

    function ChatUser(userName, displayName, avatar, currentRoom, device) {
        var self = this;
        self.userName = ko.observable(userName);
        self.displayName = ko.observable(displayName);
        self.avatar = ko.observable(avatar);
        self.currentRoom = ko.observable(currentRoom);
        self.device = ko.observable(device);
    }

    function ChatMessage(content, timestamp, from, isMine, avatar) {
        var self = this;
        self.content = ko.observable(content);
        self.timestamp = ko.observable(timestamp);
        self.from = ko.observable(from);
        self.isMine = ko.observable(isMine);
        self.avatar = ko.observable(avatar);
    }

    var viewModel = new AppViewModel();
    ko.applyBindings(viewModel);
});