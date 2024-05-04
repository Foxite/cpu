my second attempt at designing a computer. this one does define a distinct "cpu" component, which is in cpu_proc16a2.dig. the cpu also has a distinct instruction decoder component, which is in decode16a1.dig. the "16a1" suffix isn't meaningful.

there are two main "versions" of the computer:
- computer_basic.dig, which really isn't much of an improvement over computer01 beyond splitting the cpu into its own component. it's still spaghetti, although it doesn't have much pasta.
- computer_foxbay.dig (computer design is codenamed "Fox Bay"), which has mapped memory, allowing multiple devices to exist in its address space. i've given it RAM, video memory, and a button device - in theory, I could write pong on this. (but i haven't, because i don't want to code that in assembly.) in terms of spaghetti, it's more like ravioli, as every logical component is cordoned off with rectangles and tunnels are used to connect components to the bus.

the cpu component itself is more spaghetti, though.
