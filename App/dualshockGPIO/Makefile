COMPILER = g++
CFLAGS = -g -MMD -MP -Wall -Wextra -Winit-self -Wno-unused-parameter -Wfloat-equal
LDFLAGS =
LIBS =
INCLUDE = -I../../Lib
TARGET   = ./$(shell basename `readlink -f .`)
OBJDIR = ../../Bin/obj
ifeq "$(strip $(OBJDIR))" ""
	OBJDIR = .
endif
SOURCES = $(wildcard *.cpp)
OBJECTS  = $(addprefix $(OBJDIR)/, $(SOURCES:.cpp=.o))
DEPENDS  = $(OBJECTS:.o=.d)

LIBDIR = ../../Lib
EXOBJS = $(OBJDIR)/RPiGpioDrv.o $(OBJDIR)/CJoystickDrv.o

$(TARGET): $(OBJECTS) $(LIBS)
	$(COMPILER) -o $@ $^ $(LDFLAGS) $(EXOBJS)

$(OBJDIR)/%.o: %.cpp
	@[ -d $(OBJDIR) ] || mkdir -p $(OBJDIR)
	$(MAKE) -C $(LIBDIR)/drivers/RPiGpioDrv
	$(MAKE) -C $(LIBDIR)/drivers/JoystickDrv
	$(COMPILER) $(CFLAGS) $(INCLUDE) -o $@ -c $<

.PHONY: subsystem
subsystem:
	$(MAKE) -C $(LIBDIR)/drivers/RPiGpioDrv
	$(MAKE) -C $(LIBDIR)/drivers/JoystickDrv

.PHONY: all
all: clean subsystem $(TARGET)

.PHONY: clean
clean:
	rm -f $(OBJECTS) $(DEPENDS) $(TARGET)
	@rmdir --ignore-fail-on-non-empty `readlink -f $(OBJDIR)`
	$(MAKE) clean -C $(LIBDIR)/drivers/RPiGpioDrv
	$(MAKE) clean -C $(LIBDIR)/drivers/JoystickDrv

-include $(DEPENDS)
