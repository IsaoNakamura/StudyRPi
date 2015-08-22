COMPILER = g++
CFLAGS = -g -MMD -MP -Wall -Wextra -Winit-self -Wno-unused-parameter -Wfloat-equal
LDFLAGS =
LIBS = -lwiringPi -lm
INCLUDE = -I/usr/local/include
TARGET   = ./$(shell basename `readlink -f .`)
OBJDIR = ../../Bin/obj
ifeq "$(strip $(OBJDIR))" ""
	OBJDIR = .
endif
SOURCES = $(wildcard *.cpp)
OBJECTS  = $(addprefix $(OBJDIR)/, $(SOURCES:.cpp=.o))
DEPENDS  = $(OBJECTS:.o=.d)

LIBDIR = ../../Lib
EXOBJS =

$(TARGET): $(OBJECTS) $(EXOBJS)
	$(COMPILER)  $(INCLUDE) $(LIBS) -o $@ $^ $(LDFLAGS)

$(OBJDIR)/%.o: %.cpp
	@[ -d $(OBJDIR) ] || mkdir -p $(OBJDIR)
	$(COMPILER) $(CFLAGS) -o $@ -c $<

.PHONY: subsystem
subsystem:

.PHONY: all
all: clean subsystem $(TARGET)

.PHONY: clean
clean:
	rm -f $(OBJECTS) $(DEPENDS) $(TARGET)
	@rmdir --ignore-fail-on-non-empty `readlink -f $(OBJDIR)`

-include $(DEPENDS)
