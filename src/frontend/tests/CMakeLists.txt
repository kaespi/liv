cmake_minimum_required(VERSION 3.16)

project(liv_frontend-test VERSION 0.0.1 LANGUAGES CXX)

qt_add_executable(${PROJECT_NAME}
    unittests.cpp
    ../frontend.qrc
)

target_sources(${PROJECT_NAME}
    PRIVATE tst_frontend.qml
)

target_link_libraries(${PROJECT_NAME}
    PRIVATE
        Qt6::QuickTest
)
