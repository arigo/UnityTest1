import time
import struct
import tornado.web
import tornado.websocket
from tornado.options import define, options
from tornado.log import enable_pretty_logging


WEBSOCK_VERSION = 1

define("port", default=8415, help="run on the given port", type=int)


class Application(tornado.web.Application):

    def __init__(self):
        handlers = [
            (r"/websock/%d" % WEBSOCK_VERSION, WebSockHandler),
        ]
        super(Application, self).__init__(handlers)
        self.clients = {}


class WebSockHandler(tornado.websocket.WebSocketHandler):

    def open(self):
        print "opening websock"
        client = Client(self)
        other_clients = app.clients.values()
        app.clients[self] = client
        client.try_to_connect(other_clients)

    def on_close(self):
        client = app.clients.pop(self)
        client.close()
        print "closed websock"

    def on_message(self, message):
        client = app.clients[self]
        client.receive(message)


class Client(object):
    COUNTER = 0

    def __init__(self, ws):
        self.ws = ws
        self.connected_to = None
        self.counter = Client.COUNTER
        Client.COUNTER += 1
        self.keepalive()
        
    def keepalive(self):
        self.last_time = time.time()

    def try_to_connect(self, other_clients):
        potential = [(cl.last_time, cl)
                     for cl in other_clients if cl.connected_to is None]
        if potential:
            _, cl = max(potential)
            self.connect(cl)
            cl.connect(self)
    
    def connect(self, cl):
        print '%s => %s: connected' % (self.counter, cl.counter)
        self.connected_to = cl
        self.ws.write_message('\x00' * 4)

    def receive(self, message):
        cl = self.connected_to or self
        if not message.startswith('@\x00\x00\x00'):
            print '%s => %s: %s' % (self.counter, cl.counter,
                ' '.join(['%.3f' % struct.unpack("!f", message[i:i+4])
                          for i in range(0, len(message), 4)]))
        cl.ws.write_message(message)
        self.keepalive()

    def close(self):
        cl = self.connected_to
        if cl is not None:
            print '%s: closed' % (self.counter,)
            self.connected_to = None
            cl.close()


def main():
    global app
    enable_pretty_logging()
    app = Application()
    app.listen(options.port)
    print "Listening on port %d" % (options.port,)
    tornado.ioloop.IOLoop.current().start()

if __name__ == "__main__":
    main()
