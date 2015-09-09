# MakeFile for .o file
COMPILER = g++
CFLAGS = -g -MMD -MP -Wall -Wextra -Winit-self -Wno-unused-parameter -Wfloat-equal
LDFLAGS =
LIBS =
INCLUDE =
OBJDIR = ../../Bin/obj
ifeq "$(strip $(OBJDIR))" ""
  OBJDIR = .
endif
SOURCES = $(wildcard *.cpp)
OBJECTS = $(addprefix $(OBJDIR)/, $(SOURCES:.cpp=.o))
DEPENDS = $(OBJECTS:.o=.d)
TARGET = $(SOURCES:.cpp=.o)

$(TARGET): $(SOURCES)
	@[ -d $(OBJDIR) ] || mkdir -p $(OBJDIR)
	$(COMPILER) $(CFLAGS) $(INCLUDE) -c $^ -o $(OBJDIR)/$@


.PHONY: all
all: clean $(TARGET)

.PHONY: clean
clean:
	rm -f $(OBJECTS) $(DEPENDS) $(TARGET)
