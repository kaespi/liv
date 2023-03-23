cmake_minimum_required(VERSION 3.16)

project(lightweightimageviewer VERSION 0.1 LANGUAGES CXX)

qt_add_executable(liv
    main.cpp
    backend/ImgProvider.cpp
    backend/ImgProvider.h
    backend/BackendManager.cpp
    backend/BackendManager.h
    backend/FileSystemWalker.cpp
    backend/FileSystemWalker.h
)

qt_add_qml_module(liv
    URI lightweightimageviewer
    VERSION 1.0
    QML_FILES frontend/main.qml
)

set_target_properties(liv PROPERTIES
    MACOSX_BUNDLE_GUI_IDENTIFIER my.example.com
    MACOSX_BUNDLE_BUNDLE_VERSION ${PROJECT_VERSION}
    MACOSX_BUNDLE_SHORT_VERSION_STRING ${PROJECT_VERSION_MAJOR}.${PROJECT_VERSION_MINOR}
    MACOSX_BUNDLE TRUE
    WIN32_EXECUTABLE TRUE
)

target_link_libraries(liv
    PRIVATE Qt6::Quick)

install(TARGETS liv
    BUNDLE DESTINATION .
    LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR})
