cmake_minimum_required(VERSION 3.16)

include(FetchContent)

FetchContent_Declare(
  googletest
  GIT_REPOSITORY https://github.com/google/googletest.git
  GIT_TAG        release-1.11.0
)
FetchContent_MakeAvailable(googletest)
add_library(GTest::GTest INTERFACE IMPORTED)
target_link_libraries(GTest::GTest INTERFACE gtest_main)

if(RUN_CLANG_TIDY)
    find_program(CLANG_TIDY_EXE NAMES "clang-tidy")
    set(CMAKE_CXX_CLANG_TIDY "${CLANG_TIDY_EXE}")
endif()

add_subdirectory(backend)

qt_add_executable(liv
    main.cpp
)

qt_add_qml_module(liv
    URI lightweightimageviewer
    VERSION 0.0.1
    QML_FILES frontend/main.qml
)

set_target_properties(liv PROPERTIES
    MACOSX_BUNDLE_GUI_IDENTIFIER kgmw.ch
    MACOSX_BUNDLE_BUNDLE_VERSION ${PROJECT_VERSION}
    MACOSX_BUNDLE_SHORT_VERSION_STRING ${PROJECT_VERSION_MAJOR}.${PROJECT_VERSION_MINOR}
    MACOSX_BUNDLE TRUE
    WIN32_EXECUTABLE TRUE
)

target_link_libraries(liv
    PRIVATE Qt6::Quick
    PRIVATE liv_backend_lib
)

install(TARGETS liv
    BUNDLE DESTINATION ${CMAKE_INSTALL_BINDIR})
