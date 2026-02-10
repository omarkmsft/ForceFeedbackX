# ForceFeedbackX
Open source project to create force feedback support for Microsoft Flight Simulator 2024. Will be focused on supporting Microsoft Sidewinder Force Feedback 2 also know as Microsoft Sidewinder FFB2 as the first device.

If the plane that is loaded and being flow has hydraulic control surfaces, the project should use the wing sizes and control surface areas to estimate force on controls and then apply relative force to joystick based on airplanes speed, pitch, roll and altitude.

If the plane that is loaded and being flow uses fly-by-wire control system, then leverage the simulated forces in a typical Airbus joystick.

Also create a UI for the extension that shows the parameters for the force-feedback that can be adjusted.
