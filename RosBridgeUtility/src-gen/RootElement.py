class velType():
    def init(threshold ,setup):
        self.threshold = threshold
        self.setup = setup

class subscriptions():
    def init(topic):
        self.topic = topic

class network():
    def init(ipaddress ,port ,protocol):
        self.ipaddress = ipaddress
        self.port = port
        self.protocol = protocol

class threshold():
    def init(max ,min):
        self.max = max
        self.min = min

class setup():
    def init(increment ,init):
        self.increment = increment
        self.init = init

class scale():
    def init(r):
        self.r = r

class publications():
    def init(topic):
        self.topic = topic

class topic():
    def init(name ,target):
        self.name = name
        self.target = target

class queryable():
    def init(topic):
        self.topic = topic

class showState():
    def init():

class projections():
    def init(query):
        self.query = query

class velocity():
    def init(linear ,angular):
        self.linear = linear
        self.angular = angular

class laser_field():
    def init():

class visualization():
    def init(scale ,laser_field ,odometry ,showstate):
        self.scale = scale
        self.laser_field = laser_field
        self.odometry = odometry
        self.showstate = showstate

class angular():
    def init():

class rosbridge_config():
    def init(network ,subscriptions ,publications ,projections ,visualization ,velocity):
        self.network = network
        self.subscriptions = subscriptions
        self.publications = publications
        self.projections = projections
        self.visualization = visualization
        self.velocity = velocity

class visualizable():
    def init():

class query():
    def init(attribute):
        self.attribute = attribute

class linear():
    def init():

class odometry():
    def init():

