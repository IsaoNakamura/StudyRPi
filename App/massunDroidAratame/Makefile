COMPILER = g++

OPENCV_ROOT = /usr/local/include/opencv
USERLAND_ROOT = $(HOME)/GitHub/userland
RASPICAMCV_ROOT = $(HOME)/GitHub/robidouille/raspicam_cv

LDFLAGS_OPENCV = -lopencv_highgui -lopencv_core -lopencv_legacy -lopencv_video -lopencv_features2d -lopencv_calib3d -lopencv_imgproc -lpthread -lm -lopencv_ml -lopencv_objdetect

LDFLAGS_USERLAND = -L$(USERLAND_ROOT)/build/lib -lmmal_core -lmmal -l mmal_util -lvcos -lbcm_host
LDFLAGS_PICAMCV = $(LDFLAGS_OPENCV) $(LDFLAGS_USERLAND) -lX11 -lXext -lrt -lstdc++

CFLAGS = -g -MMD -MP -Wall -Wextra -Winit-self -Wno-unused-parameter -Wfloat-equal
LDFLAGS = -lwiringPi -lm $(LDFLAGS_PICAMCV) -L/usr/lib
LIBS = $(RASPICAMCV_ROOT)/libraspicamcv.a
INCLUDE = -I../../Lib -I$(OPENCV_ROOT) -I$(RASPICAMCV_ROOT)
TARGET   = ./$(shell basename `readlink -f .`)
OBJDIR = ../../Bin/obj
ifeq "$(strip $(OBJDIR))" ""
	OBJDIR = .
endif
SOURCES = $(wildcard *.cpp)
OBJECTS  = $(addprefix $(OBJDIR)/, $(SOURCES:.cpp=.o))
DEPENDS  = $(OBJECTS:.o=.d)

LIBDIR = ../../Lib
EXOBJS = $(OBJDIR)/CMassunDroid.o $(OBJDIR)/CamAngleConverter.o

$(TARGET): $(OBJECTS) $(EXOBJS) $(LIBS)
	$(COMPILER) -o $@ $^ $(LDFLAGS)

$(OBJDIR)/%.o: %.cpp
	@[ -d $(OBJDIR) ] || mkdir -p $(OBJDIR)
	$(MAKE) -C $(LIBDIR)/contents/MassunDroid
	$(COMPILER) $(CFLAGS) $(INCLUDE) -o $@ -c $<

.PHONY: subsystem
subsystem:
	$(MAKE) -C $(LIBDIR)/contents/MassunDroid

.PHONY: all
all: clean subsystem $(TARGET)

.PHONY: clean
clean:
	rm -f $(OBJECTS) $(DEPENDS) $(TARGET)
	@rmdir --ignore-fail-on-non-empty `readlink -f $(OBJDIR)`
	$(MAKE) clean -C $(LIBDIR)/contents/MassunDroid

-include $(DEPENDS)
